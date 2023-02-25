﻿using pixel_renderer;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace pixel_editor
{
    public class EditorEventHandler
    {
        protected internal static Editor Editor => Editor.Current;
        protected internal Action<EditorEvent> InspectorEventRaised;
        protected internal Queue<EditorEvent> Pending = new();
        protected internal void ExecuteAll()
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
        List<Tool> tools = new List<Tool>();
        public Editor()
        {
            EngineInstance.FromEditor = true;
            engine = new();
            current = this;

            InitializeComponent();
            Closing += OnDisable;
            MouseWheel += OnMouseWheelMoved;

            image.MouseDown += Image_MouseBtnChanged;
            image.MouseUp += Image_MouseBtnChanged;
            image.MouseMove += Image_MouseMove;

            Runtime.InspectorEventRaised += QueueEvent;
            CompositionTarget.Rendering += Update;

            tools = Tool.InitializedDerived();

            foreach (Tool tool in tools)
                tool.init_internal();

            foreach (Tool tool in tools)
                tool.Awake();

            Input.RegisterAction(SendCommandKeybind, Key.Return);
            Input.RegisterAction(ClearKeyboardFocus, Key.Escape);
            Input.RegisterAction(StartStop, Key.LeftCtrl);

            inspector = new Inspector(inspectorGrid);
            Runtime.Editor = inspector;


            Task.Run(() => Console.Print("Session Started. Type 'help();' for a list of commands.", true));

            Runtime.OnProjectSet += OnProjectSet;
            Runtime.OnStageSet += OnStageSet;

            OnStageSet(Runtime.Current.GetStage());
            OnProjectSet(Runtime.Current.LoadedProject);

            Runtime.OutputImages.Add(image);
            Runtime.ToggleRendering();

        }
        private void Update(object? sender, EventArgs e)
        {
            CMouse.Update();
            inspector.Update(sender, e);
            UpdateMetrics();
            Events.ExecuteAll();
            foreach (Tool tool in tools)
                tool.Update(1f);
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
        protected internal void EditorEvent(EditorEvent e)
        {
            e.action?.Invoke(e.args);
            if (e.message is "" || e.message.Contains("$nolog")) return;

            consoleOutput.Text += e.message + '\n';
            consoleOutput.ScrollToEnd();
        }

        #region Fields/Properties
        Vec2 mouseSelectedNodeOffset = new Vec2();
        string stageName, projectName;
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
        internal Node? Selected = null;

        public Inspector? Inspector => inspector;
        private readonly Inspector inspector;
        // for stage creation, hopefully a better solution eventually.
        private StageWnd? stageWnd;
        public readonly EditorEventHandler Events = new();
        public byte[] Frame => Runtime.Current.renderHost.GetRenderer().Frame;
        public int Stride => Runtime.Current.renderHost.GetRenderer().Stride;
        public Vec2 Resolution => Runtime.Current.renderHost.GetRenderer().Resolution;
        #endregion
        #region Input Events
        private void StartStop(object[]? obj)
        {
            if (Input.GetInputValue(0, "P"))
                Runtime.TogglePhysics();

        }
        private void ClearKeyboardFocus(object[]? obj)
        {
            Keyboard.ClearFocus();
        }
        void SendCommandKeybind(object[]? o)
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
                    Command.Call(line);
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
                Current.consoleOutput.Dispatcher.Invoke(() => Current.consoleOutput.Clear());
            Current.Events.Pending.Enqueue(e);
        }
        private void Wnd_Closed(object? sender, EventArgs e) => stageWnd = null;


        private void OnDisable(object? sender, EventArgs e)
        {
            stageWnd?.Close();
            Runtime.Current.mainWnd.Close();
            engine?.Close();
        }
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Runtime.TogglePhysics();
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

        public Action<string, ComponentEditor> OnEditorClosed = OnComponentEditorClosed;
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
            {"Static Body", Rigidbody.StaticBody },
            {"Rigid Body", Rigidbody.Standard },
            {"Animator", Animator.Standard},
            {"Light", Light.Standard},
            {"Floor", Floor.Standard},
        };
        #endregion
    }
}
