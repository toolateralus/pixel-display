using System;

using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;

using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace pixel_editor
{
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

        public Editor()
        {
            InitializeComponent();
            inspector = new Inspector(inspectorObjName,  inspectorObjInfo,  inspectorChildGrid, editorMessages);
            Runtime.inspector = inspector;
            engine = new(new Project(""));
            GetEvents(); 
        }
        private int renderStateIndex = 0;

        private readonly Inspector inspector;
        private StageWnd? stageWnd;

        internal EngineInstance? engine;
        internal RenderHost? host => Runtime.Instance.renderHost;  

        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);

            // render and update gc alloc text in editor.
            if (Runtime.Instance.IsRunning 
                && Runtime.Instance.GetStage() is not null  
                && host.State == RenderState.Scene)
                {
                    host.Render(image, Runtime.Instance);
                    gcAllocText.Content =
                        $"{Runtime.Instance.renderHost.info.GetTotalMemory()}" +
                        $" \n frame rate : {Runtime.Instance.renderHost.info.Framerate}"; 
                }
        }
        private void GetEvents()
        {
           // HandleInputMediator();
            CompositionTarget.Rendering += Update;
            Closing += OnDisable;
            image.MouseLeftButtonDown += Mouse0;

            var args = new object[]
            {
                   inspector,
                   Runtime.Instance,
            };
            Action<object[]?> expr = (o) =>
            {
                Console.Print("Action called");

                var inspector = o[0] as Inspector;
                var run = o[1] as Runtime;

                run.TrySetStageAsset(0);
                inspector.RedText(null)?.Invoke(null);
            };      

            InputAction action = new(false, expr, args, Key.G);

            Input.RegisterAction(action, Input.InputEventType.KeyDown);
        }
        private static Point ViewportPoint(Image img, Point pos)
        {
            pos.X /= img.ActualWidth;
            pos.Y /= img.ActualHeight;

            pos.X *= img.Width;
            pos.Y *= img.Height;
            return pos;
        }
        private void IncrementRenderState()
        {
            if (Runtime.Instance.GetStage() is null)
            {
                host.State = RenderState.Off;
                viewBtn.Content = "Stage null.";
                return; 
            }
            
            renderStateIndex++;
            if (renderStateIndex == sizeof(RenderState) - 1)
                renderStateIndex = 0;

            host.State = (RenderState)renderStateIndex;
            viewBtn.Content = host.State.ToString();

            if (host.State != RenderState.Game)
                return; 

            var msg = MessageBox.Show("Enter Game View?", "Game View", MessageBoxButton.YesNo);
            if (msg != MessageBoxResult.Yes)
                return;
            Runtime.Instance.mainWnd = new();
            Runtime.Instance.mainWnd.Show();
        }
        private void Wnd_Closed(object? sender, EventArgs e) =>
            stageWnd = null;    
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
            ProjectIO.SaveProject(Runtime.Instance.LoadedProject);
            Library.Sync();
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
    }

    public class EditorMessage : InspectorEvent
    {
        public EditorMessage(string message) : base(message)
        {
        }

        public static EditorMessage New(string message, object? sender = null, object[]? args = null, Action<object[]>? action = null)
        {
            message = DateTime.Now.ToLocalTime().ToShortTimeString() + " " + message; 
            return new(message)
            {
                expression = action,
                args = args,
                sender = sender,
            };
        }
    }

}