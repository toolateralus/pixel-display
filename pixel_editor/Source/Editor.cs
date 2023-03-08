using pixel_renderer;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Image = pixel_renderer.Image;

namespace pixel_editor
{
    public partial class Editor : Window
    {
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

        private void InitializeEditor()
        {
            current = this;

            //wpf init
            InitializeComponent();
            
            Importer.Import(false);
            Project project = Project.Load();
            Runtime.Initialize( project);
            
            GetEvents();
            Tools = Tool.InitializeToolkit();
            
            GetInputs();
            Console.Print(motd, true);
            
            OnStageSet(Runtime.Current.GetStage());
            OnProjectSet(Runtime.Current.project);

            Runtime.OutputImages.Add(image);

         
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
            return;
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
            Input.RegisterAction(ResetEditor, Key.F5); 
            Input.RegisterAction(Console.cmd_clear_console().Invoke, Key.F10);
            Input.RegisterAction(SendCommandKeybind, Key.Return);
            Input.RegisterAction(ClearKeyboardFocus, Key.Escape);
            Input.RegisterAction(() => OnSyncBtnPressed(null, null), Key.LeftCtrl);
            Input.RegisterAction(DestroySelected, Key.Delete);
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
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Dispose();

            current.Close();
            current = new();  
            current.Show();

            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        private void Update(object? sender, EventArgs e)
        {
            CMouse.Update();
            inspector.Update(sender, e);
            
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
                framerateLabel.Content =
                    $"last : {framerate} avg :{avg}\n min : {min} max :{max}";
            }
        }
        protected internal void EditorEvent(EditorEvent e)
        {
            e.action?.Invoke(e.args);
            if (e.message is "" || e.message.Contains("$nolog"))
            {
                if (e is FocusNodeEvent nodeEvent && nodeEvent.args.First() is Node node)
                {
                    Current.ActivelySelected.Add(node);
                    Current.LastSelected = node;
                    Inspector.DeselectNode(); 
                    Inspector.SelectNode(node);
                    StageCameraTool.TryFollowNode(node); 
                }
                return; 
            }

            if (consoleOutput.LineCount == Constants.ConsoleMaxLines)
                Console.Clear(); 

            consoleOutput.Text += e.message + '\n';
            consoleOutput.ScrollToEnd();
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
        private StageWnd? stageWnd;
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
                consoleOutput.Foreground = Brushes.Green;
                consoleOutput.Background = Brushes.Black;
            };
        }

        private void OnProjectSet(Project obj)
        {
            projectName = obj.Name;
            Current.Title = $"{projectName} : : {stageName}";
        }
        private void OnStageSet(Stage obj)
        {
            stageName = obj.Name;
            Current.Title = $"{projectName} : : {stageName}";
        }

        private void OnMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            CMouse.Refresh(e);
        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            CMouse.Refresh(e);
        }
        private void Image_MouseBtnChanged(object sender, MouseButtonEventArgs e)
        {
            CMouse.Refresh(e);
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
                    CommandParser.TryCallLine(line, Console.Current.LoadedCommands);
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
        public async static void QueueEvent(EditorEvent e)
        {
            if (e.ClearConsole)
                Current.consoleOutput.Dispatcher.Invoke(() => Current.consoleOutput.Clear());

            while (Current.Events.Pending.Count > Constants.EditorEventQueueMaxLength)
                await Task.Delay(15);

            Current.Events.Pending.Enqueue(e);
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
                AssetLibrary.Sync();
                return; 
            }
            if (!Input.Get(Key.S))
                return; 
            AssetLibrary.Sync();
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
        internal Dictionary<string, ComponentEditor?> ComponentEditors = new();
        public void RegisterComponentEditor(string name, ComponentEditor editor)
        {
            if (ComponentEditors.ContainsKey(name))
            {
                editor.Close();
                ComponentEditors[name].Focus();
                ComponentEditors[name].Refresh(ComponentEditors[name].component);
                return; 
            }
            ComponentEditors.Add(name, editor);
            ComponentEditors[name].Show();
        }

        public readonly Action<string, ComponentEditor> OnEditorClosed = OnComponentEditorClosed;
        private static void OnComponentEditorClosed(string name, ComponentEditor obj)
        {
            if (Current.ComponentEditors.ContainsKey(name))
                Current.ComponentEditors.Remove(name);
        }
           

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
                Inspector.SetRowAndColumn(addNodeContextMenu, 10, 10, 0, 0);

                int i = 0;
                foreach (var item in addNodeFunctions)
                {
                    Button button = Inspector.GetButton(item.Key, new(0, 0, 0, 0));
                    button.Name = $"button{i}";
                    addNodeActions.Add(() => AddNodePrefab(new(item.Key, item.Value)));
                    button.FontSize = 2;
                    button.Click += newNodeButtonClicked;
                    addNodeContextMenu.Children.Add(button);
                    Inspector.SetRowAndColumn(button, 2, 3, 15, i * 2);
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
                Runtime.Current.GetStage().AddNode(node);
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

