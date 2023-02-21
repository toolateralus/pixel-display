using System;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;
using System.Windows;
using System.Windows.Input;
using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Linq;

namespace pixel_editor
{
    public class EditorEventHandler
    {
        internal protected static Editor Editor => Editor.Current;
        internal protected Action<EditorEvent> InspectorEventRaised;
        internal protected Queue<EditorEvent> Pending = new();
        internal protected void ExecuteAll()
        {
            EditorEvent e;
            for (int i = 0; Pending.Count > 0; ++i)
            {
                e = Pending.Dequeue();
                if (e is null)
                    return;
                Editor.Current.EditorEvent(e);
            }
        }
    }
    public partial class Editor : Window
    {
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
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
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
        Vec2 mouseSelectedNodeOffset = new Vec2();

        public Editor()
        {
            engine = new();
            current = this;

            InitializeComponent();
            GetEvents();

            inspector = new Inspector(inspectorGrid);
            Runtime.Editor = inspector;

            Task.Run(() => Console.Print("Session Started. Type 'help();' for a list of commands.", true));

            Runtime.OnProjectSet += OnProjectSet;
            Runtime.OnStageSet += OnStageSet;
            OnStageSet(Runtime.Current.GetStage());
            OnProjectSet(Runtime.Current.LoadedProject);
            Runtime.OutputImages.Add(image);

        }
        string stageName, projectName;
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

        internal EngineInstance? engine;
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

        public Inspector? Inspector => inspector;
        private readonly Inspector inspector;
        // for stage creation, hopefully a better solution eventually.
        private StageWnd? stageWnd;
        public readonly EditorEventHandler Events = new();
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
        private static async Task<System.Drawing.Color> VerifyColorBrightnessOrGetNew(System.Drawing.Color c)
        {
            int iterations = 0;
            while (255 + c.B + c.R + c.G < 610)
            {
                c = JRandom.Color();
                await Task.Delay(1);
                iterations++;

                if (iterations > 1000)
                    break;
            }
            return c;
        }
        internal Action<object?> BlackText(object? o = null)
        {
            return (o) =>
            {
                consoleOutput.Foreground = Brushes.Green;
                consoleOutput.Background = Brushes.Black;
            };
        }
        public byte[] Frame => Runtime.Current.renderHost.GetRenderer().Frame;
        public int Stride => Runtime.Current.renderHost.GetRenderer().Stride;
        public Vec2 Resolution => Runtime.Current.renderHost.GetRenderer().Resolution;
        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);
            UpdateMetrics();
            Events.ExecuteAll();
            TryDragNode();
            TryZoomCamera();
            

        }

        private void TryZoomCamera()
        {
        }

        private void TryDragNode()
        {
            if (!CMouse.LeftPressedLastFrame && CMouse.Left)
                CMouse.LeftPressedThisFrame = true;
            else
                CMouse.LeftPressedThisFrame = false;
            CMouse.LeftPressedLastFrame = CMouse.Left;

            if (CMouse.Left && Input.GetInputValue(Key.LeftShift) && selectedNode != null)
            {
                if (CMouse.LeftPressedThisFrame)
                    mouseSelectedNodeOffset = selectedNode.Position - CMouse.GlobalPosition;

                selectedNode.Position = CMouse.GlobalPosition + mouseSelectedNodeOffset; 
            }
        }

        private void GetEvents()
        {
            Closing += OnDisable;
            MouseWheel += OnMouseWheelMoved;
            

            image.MouseLeftButtonDown += Image_Mouse0;
            image.MouseDown += Image_MouseBtnChanged;
            image.MouseUp += Image_MouseBtnChanged;
            image.MouseMove += Image_MouseMove;
            Runtime.InspectorEventRaised += QueueEvent;
            CompositionTarget.Rendering += Update; 
             
            Input.RegisterAction(SendCommandKeybind, Key.Return);
            Input.RegisterAction(ClearKeyboardFocus, Key.Escape);
            Input.RegisterAction(ToggleKeybind, Key.LeftShift);

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

        private void ToggleKeybind(object[]? obj)
        {
            if (Input.GetInputValue(0, "P"))
                Runtime.Current.Toggle();

        }

        private void ClearKeyboardFocus(object[]? obj)
        {
            Keyboard.ClearFocus();
        }
     
        void SendCommandKeybind(object[]? o)
        {
            if (!editorMessages.IsKeyboardFocusWithin)
                return;

            if (!Input.GetInputValue(InputEventType.KeyDown, "LeftShift"))
                return;

            OnCommandSent(new(), new());

            editorMessages.Clear();
        }
        

        private void UpdateMetrics()
        {
            var memory = Runtime.Current.renderHost.info.GetTotalMemory();
            var framerate = Runtime.Current.renderHost.info.Framerate;
            gcAllocText.Content =
                $"{memory}";

            framerateLabel.Content =
                $"{framerate}";

        }

        internal protected void EditorEvent(EditorEvent e)
        {
            e.action?.Invoke(e.args);
            if (e.message is "" || e.message.Contains("$nolog")) return;

            consoleOutput.Text += e.message + '\n';
            consoleOutput.ScrollToEnd();
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
                    Command.Call(line);
            }
        }

        #region UI Events
        private void Wnd_Closed(object? sender, EventArgs e) => stageWnd = null;
        Node? selectedNode = null;
        private void Image_Mouse0(object sender, MouseButtonEventArgs e)
        {
            if (!TryClickNodeOnScreen(sender, out selectedNode) || selectedNode is null)
                return; 
        }

        private bool TryClickNodeOnScreen(object sender, out Node? result)
        {
            inspector.DeselectNode();
            result = null; 
            Stage stage = Runtime.Current.GetStage();

            if (stage is null)
                return false;

            StagingHost stagingHost = Runtime.Current.stagingHost;

            if (stagingHost is null)
                return false;

            bool foundNode = stagingHost.GetNodeAtPoint(stage, CMouse.GlobalPosition, out var node);

            if (foundNode)
                inspector.SelectNode(node);

            result = node;
            return foundNode; 
        }

        private void OnDisable(object? sender, EventArgs e)
        {
            stageWnd?.Close();
            Runtime.Current.mainWnd.Close();
            engine?.Close();
        }
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Runtime.Current.Toggle();
            playBtn.Content = Runtime.Current.IsRunning ? "On" : "Off";
            playBtn.Background = Runtime.Current.IsRunning ? Brushes.LightGreen : Brushes.LightPink;
        }
        private void OnViewChanged(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is Button button)
            {
                button.FontSize = 4 ;
                button.Content = $"{Environment.MachineName}";
            }
        }
        private void OnImportBtnPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Importer.Import(true);
        }
        private void OnSyncBtnPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

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

        /// <summary>
        /// For users to pass an event to the inspector to be executed as soon as possible
        /// </summary>
        /// <param name="e"></param>
        public static void QueueEvent(EditorEvent e)
        {
            if (e.ClearConsole)
                Current.consoleOutput.Dispatcher.Invoke(() => Current.consoleOutput.Clear());
            Current.Events.Pending.Enqueue(e);
        }
        #endregion

        private void consoleOutput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        internal void SetTheme(string name)
        {
            name = name.ToLower();
        }

        bool consoleOpen = true;
        public ComponentEditor? componentEditor;

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

        bool newMenuOpen = false;
        Grid newMenuGrid;
        List<Action> AddItemActions = new();

        private void New(object sender, RoutedEventArgs e)
        {
            e.Handled = true; 
            if (!newMenuOpen)
            {
                newMenuOpen = true;
                newMenuGrid = Inspector.GetGrid();
                inspectorGrid.Children.Add(newMenuGrid);
                Inspector.SetRowAndColumn(newMenuGrid, 10, 10, 0, 0);

                int i = 0;
                foreach (var item in objects)
                {
                    Button button = Inspector.GetButton(item.Key, new(0, 0, 0, 0));
                    button.Name = $"button{i}";
                    AddItemActions.Add(() => AddObject(new(item.Key, item.Value)));
                    button.FontSize = 2;
                    button.Click += NewObjectButtonClicked;
                    newMenuGrid.Children.Add(button);
                    Inspector.SetRowAndColumn(button, 2, 3, 15, i * 2);
                    i++;

                }
                return;
            }
            newMenuOpen = false;
            newMenuGrid.Children.Clear();
            newMenuGrid = null;
        }

        private void AddObject(KeyValuePair<string, object> item)
        {
            if (item.Value is Func<Node> funct)
            {
                Runtime.Log("Node added!");
                Runtime.Current.GetStage().AddNode(funct.Invoke()); 
            }
        }

        private void NewObjectButtonClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true; 
            if (sender is not Button button) return;
            foreach (var item in objects)
                if (button.Name.ToInt() is int i && AddItemActions.Count > i)
                    AddItemActions[i]?.Invoke();
        }
        public static Dictionary<string, Func<Node>> objects = new()
        {
            {"Static Body", Rigidbody.StaticBody },
            {"Rigid Body", Rigidbody.Standard },
            {"Animator", Animator.Standard},
            {"Floor",Floor.Standard},
        };
    }
}

