using Pixel;
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Image = Pixel.Image;

namespace pixel_editor
{
    public partial class Editor : Window
    {
        SolidColorBrush framerateBrush = new(); 
        internal ComponentEditor componentEditor;
        internal FileViewer fileViewer;
        private const string motd = "Session started. Type 'help();' for a list of commands.";
        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty =
            DependencyProperty.Register(
                "ScaleValue",
                typeof(double),
                typeof(Editor),
                new UIPropertyMetadata(
                    1.0,
                    new PropertyChangedCallback(OnScaleValueChanged),
                    new CoerceValueCallback(OnCoerceScaleValue)
                    )
                );
        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            Editor mainWindow = o as Editor;
            return mainWindow != null ? mainWindow.OnCoerceScaleValue((double)value) : value;
        }
        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Editor mainWindow = o as Editor;
            mainWindow?.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }
        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);
            return value;
        }
        protected virtual void OnScaleValueChanged(double oldValue, double newValue) { }
        private void CalculateScale()
        {
            double yScale = ActualHeight / 250f;
            double xScale = ActualWidth / 200f;
            double value = Math.Min(xScale, yScale);

            ScaleValue = (double)OnCoerceScaleValue(editorWindow, value);
        }
        public void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale();
        }
        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        #endregion
        static List<Tool> Tools = new List<Tool>();
        public Editor()
        {
            InitializeEditor();
            inspector = new Inspector(inspectorGrid);
            Runtime.Current.Inspector = inspector;
        }
        public EditorSettings settings; 
        private void InitializeEditor()
        {
            if (current != null && current != this)
            {
                Close();
                return; 
            }

            current = this;
            InitializeComponent();
            
            Importer.Import(false);
           
            InitializeSettings();
            
            Project project = Project.Default;
            Runtime.Initialize(project);
           
            GetEvents();
            GetInputs();
            
            Tools = Tool.InitializeToolkit();

            OnStageSet(StagingHost.Standard());

            OnProjectSet(Runtime.Current.project);

            fileViewer = new(FileViewerGrid, fileViewerListBox);

            Console.Print(motd);
            Runtime.OutputImages.Add(image);
        }
        private static void InitializeSettings()
        {
            Metadata meta = new(Constants.WorkingRoot + "\\obj\\editorSettings.asset");
            EditorSettings settings = IO.ReadJson<EditorSettings>(meta);
            if (settings is null)
            {
                settings = new("temp"+UUID.NewUUID());
                IO.WriteJson(settings, meta);
            }
            Current.settings = settings;
        }
        public static List<Node> DuplicateSelected(bool all = true)
        {
            List<Node> result = new();
            //return result;
            // OUT OF COMISSION, THIS DOES REALLY ODD STUFF Such As
            // Makes a portion of the background black/missing depending on zoom
            // Makes nodes render as if theyre almost to the max float value in each position vector, just really jittery scribbly odd shaped sprites and stuff.
            return result;

            if (Input.Get(Key.D))
            {
                if (Current.LastSelected != null || Current.ActivelySelected.Count > 0)
                {
                    Node selected = Current.LastSelected?.Clone();
                    if (selected != null)
                    {
                        selected.UUID = UUID.NewUUID(); 
                        selected.Position += Vector2.One;
                        result.Add(selected);
                    }

                    if (all)
                        for (int i = 0; i < Current.ActivelySelected.Count; i++)
                        {
                            Node? node = Current.ActivelySelected[i];
                            if (node.Clone() is Node clone)
                            {
                                clone.UUID = UUID.NewUUID(); 
                                clone.Position += Vector2.One;
                                result.Add(clone);
                            }
                        }

                    current.ActivelySelected.AddRange(result);
                }
            }
            return result; 
        }
        public static void DestroySelected()
        {
            if (Current.LastSelected != null || Current.ActivelySelected.Count > 0)
            {
                Current.LastSelected?.Destroy();

                foreach (var selected in Current.ActivelySelected)
                {
                    if (selected is not null)
                    {
                        Console.Print($"Node : {selected.Name} destroyed.", true);
                        selected.Destroy();
                    }
                    
                }
            }
        }
        private void GetEvents()
        {
            Closing += OnDisable;
            MouseWheel += OnMouseWheelMoved;

            image.MouseDown += Image_MouseBtnChanged;
            image.MouseUp += Image_MouseBtnChanged;
            image.MouseMove += Image_MouseMove;

            Runtime.InspectorEventRaised += QueueEvent;
            CompositionTarget.Rendering += Update;

            Runtime.OnProjectSet += OnProjectSet;
            Runtime.OnStageSet += OnStageSet;
        }
        private void UnsubscribeEvents()
        {
            Closing -= OnDisable;

            MouseWheel -= OnMouseWheelMoved;

            image.MouseDown -= Image_MouseBtnChanged;
            image.MouseUp -= Image_MouseBtnChanged;
            image.MouseMove -= Image_MouseMove;

            Runtime.InspectorEventRaised -= QueueEvent;
            CompositionTarget.Rendering -= Update;

            Runtime.OnProjectSet -= OnProjectSet;
            Runtime.OnStageSet -= OnStageSet;
        }
        private void GetInputs()
        {
            Input.RegisterAction(this, ResetEditor, Key.F5); 
            Input.RegisterAction(this, Console.cmd_clear_console().Invoke, Key.F10);
            Input.RegisterAction(this, SendCommandKeybind, Key.Return);
            Input.RegisterAction(this, ClearKeyboardFocus, Key.Escape);
            Input.RegisterAction(this, () => OnSyncBtnPressed(null, null), Key.LeftCtrl);
            Input.RegisterAction(this, DestroySelected, Key.Delete);
            Input.RegisterAction(this, TryDuplicate, Key.D);
        }
        private void TryDuplicate()
        {
            if (!Input.Get(Key.LeftCtrl))
                return;

            Runtime.Current.GetStage()?.AddNodes(DuplicateSelected(true));
        }
        public void Dispose()
        {
            UnsubscribeEvents();
            Runtime.Current.Dispose();
            Tools.Clear();
        }
        private void ResetEditor()
        {
            if(Runtime.IsRunning)
                Runtime.Toggle(); 
            Application.Current.Shutdown();

            return; 
            // Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Dispose();

            current.Close();
           // Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

        }
        private void Update(object? sender, EventArgs e)
        {
            CMouse.Update();

            UpdateMetrics();
            
            Events.ExecuteAll();

            foreach (Tool tool in Tools)
                tool.Update(1f);
        }
        private void UpdateMetrics()
        {
            RenderHost renderHost = Runtime.Current.renderHost;
            RenderInfo info = renderHost.info;

            if (info.frameCount % 60 == 0)
            {
                var framerate = info.Framerate;
                var min = info.lowestFrameRate;
                var max = info.highestFrameRate;
                var avg = info.averageFrameRate;

                switch (avg) 
                {
                    case < 10:
                        framerateBrush = Brushes.DarkRed;
                        break;
                    case < 20:
                        framerateBrush = Brushes.Red;
                        break;
                    case < 30:
                        framerateBrush = Brushes.DarkOrange;
                        break;
                    case < 40:
                        framerateBrush = Brushes.Orange;
                        break;
                    case < 50:
                        framerateBrush = Brushes.Yellow;
                        break;
                    case < 60:
                        framerateBrush = Brushes.White;
                        break;
                    case > 60:
                        framerateBrush = Brushes.Green;
                        break;
                }

                framerateLabel.Foreground = framerateBrush;
                framerateLabel.Content =
                    $"last : {framerate} avg :{avg}\n min : {min} max :{max}";
            }
        }
        public void fv_selection_changed(object? sender, EventArgs e)
        {
        }
        #region Fields/Properties
        string stageName, projectName;
        internal static RenderHost? Host => Runtime.Current.renderHost;

        private static Editor current;
        public static Editor Current
        {
            get
            {
                if (current is not null)
                    return current;
                else current = new();
                return current;
            }
        }
        internal Node? LastSelected = null;
        internal List<Node> ActivelySelected = new();

        public Inspector? Inspector => inspector;
        private readonly Inspector inspector;

        // for stage creation, hopefully a better solution eventually.
        internal StageWnd? stageWnd;

        public readonly EditorEventHandler Events = new();
        public byte[] Frame => Runtime.Current.renderHost.GetRenderer().Frame;
        public int Stride => Runtime.Current.renderHost.GetRenderer().Stride;
        public Vector2 Resolution => Runtime.Current.renderHost.GetRenderer().Resolution;
        #endregion
        #region Input Events
     
        private void ClearKeyboardFocus()
        {
            Keyboard.ClearFocus();
        }
        void SendCommandKeybind()
        {
            if (!editorMessages.IsKeyboardFocusWithin)
                return;

            OnCommandSent(new(), new());

            editorMessages.Clear();
        }
        #endregion
        #region UI Events

        internal Action<object?> RedText(object? o = null)
        {
            return (o) =>
            {
                consoleOutput.Foreground = Brushes.Red;
            };
        }
        /// <summary>
        /// if the color's brightness is under 610 (a+r+g+b) this continually gets a new color every ms for 1000 ms or until it's bright enough then returns it
        /// </summary>
        /// <param name="c"></param>
        /// <returns>A color with a brightness value greater than 610 </returns>
        internal Action<object?> BlackText(object? o = null)
        {
            return (o) =>
            {
                consoleOutput.Foreground = Brushes.White;
                consoleOutput.Background = Brushes.Black;
            };
        }
        private void OnProjectSet(Project obj)
        {
            projectName = obj.Name;
            Console.Print("Project " + projectName + " set.");
            Current.Title = $"{projectName} : : {stageName}";
        }
        private void OnStageSet(Stage obj)
        {
            if (obj is null)
            {
                Console.Print("Stage was null, not set.");
                return;
            }


            stageName = obj.Name;
            Console.Print("Stage " + stageName + " set.");
            Current.Title = $"{projectName} : : {stageName}";
        }
        private void OnMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            CMouse.MouseWheelRefresh(e);
        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            CMouse.RefreshMousePosition(e);
        }
        private void Image_MouseBtnChanged(object sender, MouseButtonEventArgs e)
        {
            CMouse.MouseButtonRefresh(e);
        }
        private async void OnCommandSent(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent != null)
                e.Handled = true;

            // the max amt of lines that a script can consist of
            int cap = 5;

            int lineCt = editorMessages.LineCount - 1;

            if (lineCt < cap)
                cap = lineCt;

            for (int i = 0; i < cap; ++i)
            {

                string line = editorMessages.GetLineText(i);

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.Contains("wait(") && (line.Contains(')') || line.Contains(");")))
                {
                    CommandParser.ParseArguments(line, out string[] args, out _);
                    int delayMs = args[0].ToInt();
                    if (delayMs == -1)
                    {
                        Command.Error("wait(int delay);", 0);
                        continue;
                    }
                    Runtime.Log($"waiting for {delayMs} ms");
                    await Task.Delay(i);
                    Runtime.Log($"done waiting");
                    continue;
                }

                if (line != "")
                    CommandParser.TryCallLine(line);
            }
        }
        private void OnToggleConsole(object sender, RoutedEventArgs e)
        {
            if (!consoleOpen)
            {
                consoleOpen = true;

                editorMessages.Visibility = Visibility.Collapsed;
                consoleOutput.Visibility = Visibility.Collapsed;

                return;
            }

            editorMessages.Visibility = Visibility.Visible;
            consoleOutput.Visibility = Visibility.Visible;

            consoleOpen = false;
        }
        /// <summary>
        /// For users to pass an event to the inspector to be executed as soon as possible
        /// </summary>
        /// <param name="e"></param>
        public static void QueueEvent(EditorEvent e)
        {
            if (e.ClearConsole)
                Current.consoleOutput.Dispatcher.Invoke(() => Current.consoleOutput.Items.Clear());
            Current.Events.Pending.Push(e);
        }
        private void Wnd_Closed(object? sender, EventArgs e) => stageWnd = null;
        private void OnDisable(object? sender, EventArgs e)
        {
            stageWnd?.Close();
        }
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Runtime.Toggle(); 
            playBtn.Content = Runtime.IsRunning ? "On" : "Off";
            playBtn.Background = Runtime.IsRunning ? Brushes.LightGreen : Brushes.LightPink;
        }
        private void OnViewChanged(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is Button button)
            {
                button.FontSize = 4;
                button.Content = $"{Environment.MachineName}";
            }
        }
        private void OnImportBtnPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
        private void OnSyncBtnPressed(object sender, RoutedEventArgs e)
        {
            if (e != null && e.RoutedEvent != null)
            {
                e.Handled = true;
                Library.SaveAll();
                return; 
            }
            if (!Input.Get(Key.S))
                return; 
            Library.SaveAll();
        }
        private void OnToggleFileViewer(object sender, RoutedEventArgs e)
        {
            if(e.RoutedEvent != null)
                e.Handled = true;

            if (FileViewerGrid.Visibility == Visibility.Hidden)
                FileViewerGrid.Visibility = Visibility.Visible;
            else FileViewerGrid.Visibility = Visibility.Hidden;
        }
        private void OnStagePressed(object sender, RoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            stageWnd = new StageWnd(this);
            stageWnd.Show();
            stageWnd.Closed += Wnd_Closed;
        }
        private void OnImportFileButtonPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Importer.ImportAssetDialog();
        }
        private void OnLoadProjectPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Project project = Project.Load();
            if (project is not null)
                Runtime.Current.SetProject(project);
        }

        #endregion
        #region Add Node Menu

        bool consoleOpen = true;
        bool addNodeContextMenuOpen = false;
        Grid addNodeContextMenu;
        List<Action> addNodeActions = new();

        private void NewNodeButtonPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (!addNodeContextMenuOpen)
            {
                addNodeContextMenuOpen = true;
                addNodeContextMenu = Inspector.GetGrid();
                inspectorGrid.Children.Add(addNodeContextMenu);
                Inspector.SetRowAndColumn(addNodeContextMenu, 15, 15, 15, 15);

                int i = 0;
                foreach (var item in addNodeFunctions)
                {
                    Button button = Inspector.GetButton(item.Key, new(0, 0, 0, 0));
                    button.Name = $"button{i}";
                    addNodeActions.Add(() => AddNodePrefab(new(item.Key, item.Value)));
                    button.FontSize = 2;
                    button.Click += newNodeButtonClicked;
                    addNodeContextMenu.Children.Add(button);
                    Inspector.SetRowAndColumn(button, 2, 3, 4, i * 2 + 4);
                    i++;

                }
                return;
            }
            addNodeContextMenuOpen = false;
            addNodeContextMenu.Children.Clear();
            addNodeContextMenu = null;
        }
        private void AddNodePrefab(KeyValuePair<string, object> item)
        {
            if (item.Value is Func<Node> funct)
            {
                Runtime.Log("Node added!");
                Node node = funct.Invoke();
                node.Position = CMouse.LastClickGlobalPosition;
                Runtime.Current.GetStage()?.AddNode(node);
                return;
            }
        }
        private void newNodeButtonClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button button) return;
            foreach (var item in addNodeFunctions)
            {
                if (button.Name.ToInt() is int i && addNodeActions.Count > i)
                    addNodeActions[i]?.Invoke();
                return; 
            }
        }
        public static Dictionary<string, Func<Node>> addNodeFunctions = new()
        {
            {"Static Body", Rigidbody.StaticBody},
            {"Rigid Body", Rigidbody.Standard},
            {"Soft Body", Softbody.SoftBody},
            {"Animator", Animator.Standard},
            {"Light", Light.Standard},
            {"Floor", Floor.Standard},
            {"UI Image", Image.Standard},
        };
        #endregion
    }
}

