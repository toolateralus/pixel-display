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

        internal EngineInstance? engine;
        internal RenderHost? Host => Runtime.Instance.renderHost;  

        private readonly Inspector inspector;
        private StageWnd? stageWnd;
        private int renderStateIndex = 0;

        public Editor()
        {
            InitializeComponent();
            inspector = new Inspector(inspectorObjName,  inspectorObjInfo,  inspectorChildGrid);
            Runtime.inspector = inspector;
            engine = new();
            GetEvents();
        }
        private void GetEvents()
        {
            CompositionTarget.Rendering += Update;
            Closing += OnDisable;
            image.MouseLeftButtonDown += Mouse0;
        }
        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);
            if (Runtime.Instance.IsRunning && Runtime.Instance.stage is not null  && Host.State == RenderState.Scene)
            {
                Host.Render(image);
                gcAllocText.Content =
                    $"{Runtime.Instance.renderHost.RenderInfo.GetTotalMemory()}" +
                    $" \n frame rate : {Runtime.Instance.renderHost.RenderInfo.Framerate}"; 
            }
        }
        private void OnDisable(object? sender, EventArgs e)
        {
            stageWnd?.Close(); 
            engine?.Close();
        }

        private void OnViewChanged(object sender, RoutedEventArgs e) => IncrementRenderState();
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            Runtime.            Instance.Toggle();  
            if (Runtime.Instance.IsRunning)
            {
                playBtn.Background = Brushes.LightGreen;
            }
            else playBtn.Background = Brushes.LightPink; 
        }
        private void IncrementRenderState()
        {
            if (Runtime.Instance.stage is null)
            {
                Host.State = RenderState.Off;
                viewBtn.Content = "Stage null.";
            }
            renderStateIndex++;
            if (renderStateIndex == sizeof(RenderState) - 1)
            {
                renderStateIndex = 0;
            }
            Host.State = (RenderState)renderStateIndex;
            viewBtn.Content = Host.State.ToString();
            if (Host.State == RenderState.Game)
            {
                var msg = MessageBox.Show("Enter Game View?", "Game View", MessageBoxButton.YesNo);
                if (msg != MessageBoxResult.Yes)
                    return;
                Runtime.                Instance.mainWnd = new();
                Runtime.                Instance.mainWnd.Show();
            }
        }
        private void Mouse0(object sender, MouseButtonEventArgs e)
        {
            // this cast could be causing erroneous behavior
            Point pos = e.GetPosition((Image)sender);

            if (Runtime.Instance.IsRunning)
            {
                inspector.DeselectNode();
                if (Runtime.Instance.stagingHost.GetNodeAtPoint(Runtime.Instance.stage, pos, out Node node))
                    inspector.SelectNode(node);
            }
        }
        private void OnImportBtnPressed(object sender, RoutedEventArgs e) => _ = Importer.ImportAsync(true);
        private void OnSyncBtnPressed(object sender, RoutedEventArgs e)
        {
            ProjectIO.SaveProject(Runtime.Instance.LoadedProject);
            Library.Sync();
        }
        private void OnImportFileButtonPressed(object sender, RoutedEventArgs e)
        {
            Importer.ImportAssetDialog();
        }
        private void OnStagePressed(object sender, RoutedEventArgs e)
        {
            stageWnd = new StageWnd(this);
            stageWnd.Show();
            stageWnd.Closed += Wnd_Closed;
        }
        private void Wnd_Closed(object? sender, EventArgs e) =>
            stageWnd = null;
        private void OnLoadProjectPressed(object sender, RoutedEventArgs e)
        {
            Project project = Project.LoadProject();
            if (project is not null)
                Runtime.Instance.SetProject(project);
        }
    }
}