using System;

using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;

using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using static pixel_renderer.Input;
using System.Linq;
using System.Windows.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;

namespace pixel_editor
{


    public class EditorEventHandler
    {
        public static Editor Editor => Editor.Current;
        public Action<InspectorEvent> InspectorEventRaised;
        public Queue<InspectorEvent> Pending = new();
        public object[] ExecuteAll()
        {
            InspectorEvent e;
            List<object> output = new(); 
            for (int i = 0; Pending.Count > 0; ++i)
            {
                e = Pending.Dequeue();
                e.action?.Invoke(e.args);
                output.Add(e);

                if (e.message.Contains("$nolog")) 
                    continue;
                Editor.Current.PrintToConsole(e);
            }
            return output.ToArray(); 
        }
    }
    public partial class Editor : Window
    {
        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(Editor), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));
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

            ScaleValue = (double)OnCoerceScaleValue(window, value);
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
            InitializeComponent();
            inspector = new Inspector(inspectorObjName, inspectorObjInfo, inspectorChildGrid);
            
            Runtime.inspector = inspector;
            Project defaultProject = new("Default");

            engine = new(defaultProject);
            
            GetEvents();
            SubscribeInputs();
        }
        internal EngineInstance? engine;
        internal static RenderHost? Host => Runtime.Instance.renderHost;

        private static Editor current = new();
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
                editorMessages.Foreground = Brushes.Red;
                editorMessages.Background = Brushes.Black;
            };
        }
        internal Action<object?> BlackText(object? o = null)
        {
            return (o) =>
            {
                editorMessages.Foreground = Brushes.Black;
                editorMessages.Background = Brushes.DarkSlateGray;
            };
        }

        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);

            if (ShouldUpdate)
            {
                Host?.Render(image);
                UpdateMetrics();
                _ = Events.ExecuteAll(); 
            }

        }
        private void GetEvents()
        {
            Closing += OnDisable;
            image.MouseLeftButtonDown += Mouse0;
            CompositionTarget.Rendering += Update;
            Runtime.InspectorEventRaised += QueueEvent;
        }
        private static void SubscribeInputs()
        {
            var action = Command.reload_stage.action;
            var args = Command.reload_stage.args;


            InputAction resetStage = new(false, action, args, Key.D1);
            RegisterAction(resetStage, InputEventType.DOWN);
        }
        private void IncrementRenderState()
        {
            if (Runtime.Instance.GetStage() is null)
            {
                Host.State = RenderState.Off;
                viewBtn.Content = "Stage null.";
                return;
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
                $"{memory}" +
                $" \n frame rate : {framerate}";
        }
        public void PrintToConsole(InspectorEvent e)
        {
            consoleOutput.AppendText(
                $"\n - {e.message}" +
                $" \n -_- - - - - - -_ - - _- - - - - - - -_- " +
                $"{DateTime.Now.ToLongDateString()}");
        }
        private static Point ViewportPoint(Image img, Point pos)
        {
            pos.X /= img.ActualWidth;
            pos.Y /= img.ActualHeight;

            pos.X *= img.Width;
            pos.Y *= img.Height;
            return pos;
        }
        private void OnCommandSent(object sender, RoutedEventArgs e)
        {
            int cap = 5;
            string[] split = editorMessages.Text.Split('\n');
            
            if (split.Length < cap)
                cap = split.Length;

            for (int i = 0; i < cap ; ++i)
            {
                var line = editorMessages.GetLineText(i);
                Command.Call(line);
            }
        }

        #region UI Events
        private void Wnd_Closed(object? sender, EventArgs e) => stageWnd = null;
        private void Mouse0(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            Point pos = e.GetPosition(img);
            pos = ViewportPoint(img, pos);
            inspector.DeselectNode();
            Stage stage = Runtime.Instance.GetStage();
            if (stage is null) return;
            StagingHost stagingHost = Runtime.Instance.stagingHost;
            if (stagingHost is null) return;

            bool foundNode = stagingHost.GetNodeAtPoint(stage, pos, out var node);
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
            
            Project? proj;
            Metadata meta;
            GetProjectPsuedoMetadata(out proj, out meta);
            ProjectIO.WriteProject(proj, meta);
            AssetLibrary.Sync();
        }
        private static void GetProjectPsuedoMetadata(out Project? proj, out Metadata meta)
        {
            proj = Runtime.Instance.LoadedProject;
            var projDir = pixel_renderer.Constants.ProjectsDir;
            var rootDir = pixel_renderer.Constants.WorkingRoot;
            var ext = pixel_renderer.Constants.ProjectFileExtension;
            var path = rootDir + projDir + '\\' + proj.Name + ext;
            meta = new Metadata(proj.Name, path, ext);
        }
        private void OnStagePressed(object sender, RoutedEventArgs e)
        {
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
            Project project = Project.LoadProject();
            if (project is not null)
                Runtime.Instance.SetProject(project);
        }

        internal static void QueueEvent(InspectorEvent e)
        {
           Current.Events.Pending.Enqueue(e);
        }
        #endregion
    }
    public class Command
    {
        public static Command reload_stage = new()
        {
            phrase = "reload|Reload|realod|/r",
            action = (o) =>
            {
                Console.Print("Josh Is Cool!");
                Runtime.Instance.ResetCurrentStage();
            },
            args = null
        };
        public static Command spawn_generic = new()
        {
            phrase = "genericNode|spawnGeneric|/sgn|++n",
            action = (o) => Runtime.Instance.GetStage().create_generic_node(),
            args = null
        };
        public static readonly Command[] Active = new Command[]
        {
            reload_stage,
            spawn_generic,
        };
        public string phrase = "";
        public Action<object[]?>? action;
        public object[]? args;
        public bool Equals(string input)
        {
            string newPhrase = "";

            if (input.Contains('$'))
                newPhrase = input.Split('$')[0];
            else newPhrase = input; 

            var split = phrase.Split('|');

            foreach (var line in split)
                if (line.Equals(input))
                    return true;
            return false; 
        }

        public void Execute()
        {
            action?.Invoke(args);
        }

        internal static void Call(string line)
        {
            foreach (var command in Active)
                if (command.Equals(line))
                {
                    int count = 0;
                    
                    if (line.Contains('$'))
                        count = line.ToInt();

                    if (count == 0)
                    {
                        command.Execute();
                        return; 
                    }
                    for(int i = 0; i < count; ++i)
                        command.Execute();
                }
        }
    }
 
}

