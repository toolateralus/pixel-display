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

        private int renderStateIndex = 0;
        private static bool ShouldUpdate =>
            Runtime.Instance.IsRunning &&
            Runtime.Instance.GetStage() is not null &&
            Host?.State == RenderState.Scene;

        public Editor()
        {
            engine = new();
            current = this;

            InitializeComponent();
            GetEvents();

            inspector = new Inspector(inspectorGrid);
            Runtime.inspector = inspector;

            Task.Run(() => Console.Print("Session Started. Type 'help();' for a list of commands.", true));

        }
        internal EngineInstance? engine;
        internal static RenderHost? Host => Runtime.Instance.renderHost;

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

        DispatcherTimer timer = new();
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
        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);

            if (ShouldUpdate)
            {
                Host?.Render(image);
                UpdateMetrics();
            }
            Events.ExecuteAll();
        }
        private void GetEvents()
        {
            Closing += OnDisable;
            image.MouseLeftButtonDown += Mouse0;
            StartEditorRenderClock();
            timer.Tick += Update;
            Runtime.InspectorEventRaised += QueueEvent;


            Input.RegisterAction(SendCommandKeybind, Key.Return);
            Input.RegisterAction(ClearKeyboardFocus, Key.Escape);
            Input.RegisterAction(ToggleKeybind, Key.LeftShift);

        }

        private void ToggleKeybind(object[]? obj)
        {
            if (Input.GetInputValue(0, "P"))
                Runtime.Instance.Toggle();

        }

        private void ClearKeyboardFocus(object[]? obj)
        {
            Keyboard.ClearFocus();
        }

        private void StartEditorRenderClock()
        {
            timer.Interval = TimeSpan.FromTicks(1000);
            timer.Start();
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
        private void IncrementRenderState()
        {
            if (Runtime.Instance.GetStage() is null)
            {
                Console.cmd_reload_stage().Invoke();


                if (Runtime.Instance.GetStage() is null)
                {
                    Console.Error("Stage was null!! Please create a new one.", 3);
                    Current.OnStagePressed(this, null);
                }


            }

            renderStateIndex++;
            if (renderStateIndex == sizeof(RenderState) - 1)
                renderStateIndex = 0;

            Host.State = (RenderState)renderStateIndex;
            viewBtn.Content = Host.State.ToString();

            if (Host.State != RenderState.Game)
                return;

            var msg = MessageBox.Show("Enter Game View?", "Game View", MessageBoxButton.YesNo);
            if (msg != MessageBoxResult.Yes)
                return;

            Runtime.Instance.mainWnd = new();
            Runtime.Instance.mainWnd.Show();
        }

        private void UpdateMetrics()
        {
            var memory = Runtime.Instance.renderHost.info.GetTotalMemory();
            var framerate = Runtime.Instance.renderHost.info.Framerate;
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

        private static Point GetNormalizedPoint(Image img, Point pos)
        {
            pos.X /= img.ActualWidth;
            pos.Y /= img.ActualHeight;
            return pos;
        }

        #region UI Events
        private void Wnd_Closed(object? sender, EventArgs e) => stageWnd = null;
        private void Mouse0(object sender, MouseButtonEventArgs e)
        {
            inspector.DeselectNode();

            Stage stage = Runtime.Instance.GetStage();

            if (stage is null)
                return;

            StagingHost stagingHost = Runtime.Instance.stagingHost;

            if (stagingHost is null)
                return;

            UIElement img = (UIElement)sender;
            Point pos = Mouse.GetPosition(img);

            pos = GetNormalizedPoint((Image)img, pos);

            Node cameraNode = stage.FindNodeWithComponent<Camera>();

            if (cameraNode is null)
                return;

            Camera cam = cameraNode.GetComponent<Camera>();

            if (cam is null)
                return;

            Vec2 globalPosition = cam.ScreenViewportToGlobal((Vec2)pos);

            bool foundNode = stagingHost.GetNodeAtPoint(stage, globalPosition, out var node);

            if (foundNode)
                inspector.SelectNode(node);

        }
        private void OnDisable(object? sender, EventArgs e)
        {
            stageWnd?.Close();
            Runtime.Instance.mainWnd.Close();
            engine?.Close();
        }
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Runtime.Instance.Toggle();
            playBtn.Content = Runtime.Instance.IsRunning ? "On" : "Off";
            playBtn.Background = Runtime.Instance.IsRunning ? Brushes.LightGreen : Brushes.LightPink;
        }
        private void OnViewChanged(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            IncrementRenderState();
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
                Runtime.Instance.SetProject(project);
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
                    AddItemActions.Add(() => AddObject(item));
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
            if (item.Value.GetType() == typeof(Node))
            {
                Runtime.Log("Node added!");
                Runtime.Instance.GetStage().AddNode((Node)item.Value); 
            }


        }

        private void NewObjectButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            foreach (var item in objects)
            {
                if (button.Name.ToInt() is int i && AddItemActions.Count > i)
                {
                    AddItemActions[i]?.Invoke();
                }
            }
        }
        public static Dictionary<string, object> objects = new()
        {
            {"Node", Rigidbody.Standard() },
        };
    }



  
}

