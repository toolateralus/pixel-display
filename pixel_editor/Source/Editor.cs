using Pixel;
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Image = Pixel.Image;
using PixelLang.Tools;
using System.Windows.Threading;
using System.Reflection;
using System.Linq;
using Pixel.Types.Components;

namespace Pixel_Editor
{
    public partial class Editor : Window
    {
        public static Action? EditorUpdate;
        public EditorInputEvents input = new();
        internal EditorViewModel viewModel;
        SolidColorBrush framerateBrush = new(); 
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
            if (current != null && current != this)
            {
                Close();
                return;
            }

            current = this;
            InitializeComponent();
            viewModel = new();

            Importer.Import(false);

            InitializeSettings();

            Project project = Project.Default;
            Runtime.Initialize(project);

            GetEvents();
            GetInputs();

            Tools = Tool.InitializeToolkit();

            OnStageSet(StagingHost.Standard());

            OnProjectSet(Runtime.Current.project);

            fileViewer = new();

            InterpreterOutput.Stream += (obj) => Console.Print(obj);
            InterpreterOutput.DebugMetrics += ConsoleControl.ShowMetrics;
            InterpreterOutput.OnClearRequested += ConsoleControl.ClearAll;
            DataContext = viewModel;

            foreach (var item in Runtime.AllComponents)
            {
                var methods = item.GetMethods();
                foreach (var method in methods)
                {
                    if (method.GetParameters().Any())
                        continue;

                    if (method.Name == "Standard")
                    {
                        addNodeFunctions[item.Name] = (Func<Node>)method.CreateDelegate(typeof(Func<Node>));
                        break;
                    }
                }
            }

        }
        public EditorSettings settings;
        private TabItem draggedTabItem;
        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the TabItem that is being dragged
            draggedTabItem = sender as TabItem;
        }
        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedTabItem != null && e.LeftButton == MouseButtonState.Pressed)
            {
                // Start the drag-and-drop operation
                DragDrop.DoDragDrop(draggedTabItem, draggedTabItem, DragDropEffects.Move);
            }
        }
        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            // Get the target DockPanel
            DockPanel targetDockPanel = sender as DockPanel;

            if (targetDockPanel != null && e.Data.GetDataPresent(typeof(TabItem)))
            {
                // Get the TabItem that is being dropped
                TabItem droppedTabItem = e.Data.GetData(typeof(TabItem)) as TabItem;

                // Remove the TabItem from its current DockPanel
                DockPanel sourceDockPanel = VisualTreeHelper.GetParent(droppedTabItem) as DockPanel;
                sourceDockPanel.Children.Remove(droppedTabItem);

                // Add the TabItem to the target DockPanel
                targetDockPanel.Children.Add(droppedTabItem);
            }
        }
       
        private static void InitializeSettings()
        {
            Metadata meta = new(Constants.WorkingRoot + "\\obj\\editorSettings.asset");
            EditorSettings settings = IO.ReadJson<EditorSettings>(meta);
            if (settings is null)
            {
                settings = new("editorSettings", false);
                settings.Metadata = meta;
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
            Current.ActivelySelected.Clear();
            Current.LastSelected = null;
        }
        private void GetEvents()
        {
            Runtime.InspectorEventRaised += EditorEventHandler.QueueEvent;
            CompositionTarget.Rendering += Update;

            Runtime.OnProjectSet += OnProjectSet;
            Runtime.OnStageSet += OnStageSet;
        }
        private void UnsubscribeEvents()
        {
            Runtime.InspectorEventRaised -= EditorEventHandler.QueueEvent;
            CompositionTarget.Rendering -= Update;

            Runtime.OnProjectSet -= OnProjectSet;
            Runtime.OnStageSet -= OnStageSet;
        }
        private void GetInputs()
        {
            Input.RegisterAction(this, () => OnSyncBtnPressed(null, null), Key.LeftCtrl);
            Input.RegisterAction(this, DestroySelected, Key.Delete);
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

            EditorUpdate?.Invoke();
            
            Events.ExecuteAll();

            foreach (Tool tool in Tools)
                tool.Update(1f);
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
        private WeakReference<Node?> lastSelected = new(null);
        internal Node? LastSelected
        {
            get
            {
                if (lastSelected.TryGetTarget(out var result))
                    return result;
                return null;
            }

            set
            {
                lastSelected = new(value);

            }
        }
        internal List<Node> ActivelySelected = new();
        public readonly EditorEventHandler Events = new();
        public byte[] Frame => Runtime.Current.renderHost.GetRenderer().Frame;
        public int Stride => Runtime.Current.renderHost.GetRenderer().Stride;
        public Vector2 Resolution => Runtime.Current.renderHost.GetRenderer().Resolution;

        #endregion
        #region Input Events

        private void ClearKeyboardFocus()
        {
            Dispatcher.Invoke(Keyboard.ClearFocus);
        }
        #endregion
        #region UI Events

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
        /// <summary>
        /// For users to pass an event to the inspector to be executed as soon as possible
        /// </summary>
        /// <param name="e"></param>
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

        List<Action> addNodeActions = new();
        internal GameView gameView = null;
        private void OnToggleGameView(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent != null)
                e.Handled = true;
            
            if(gameView is null)
                gameView = new(this);
            else
            {
                gameView.Close();
            }
        }
        private void NewNodeButtonPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button button || button.ContextMenu is not ContextMenu menu) 
                return;

            // Clear the existing items
            menu.Items.Clear();

            // Add new items from the list
            int i = 0;
            foreach (var item in addNodeFunctions)
            {
                MenuItem menuItem = new MenuItem { Header = item.Key };
                addNodeActions.Add(() => AddNodePrefab(new(item.Key, item.Value)));
                menuItem.Name = $"button{i}";
                menuItem.Click += OnNewContextClicked;
                menu.Items.Add(menuItem);
                i++;
            }

            // Open the context menu
            menu.IsOpen = true;
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
        private void OnNewContextClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not MenuItem menuItem) return;
            foreach (var _ in addNodeFunctions)
            {
                if (menuItem.Name.ToInt() is int i && addNodeActions.Count > i)
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
        internal System.Windows.Controls.Image mainImage;
        #endregion
    }
}

