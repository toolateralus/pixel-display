using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;


namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for StageWnd.xaml
    /// </summary>
    public partial class StageWnd : Window
    {
        bool usingStarterAssets = false;
        Metadata? m_background;
        Bitmap? image; 
        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(StageWnd), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));
        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            StageWnd mainWindow = o as StageWnd;
            return mainWindow != null ? mainWindow.OnCoerceScaleValue((double)value) : value;
        }
        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            StageWnd mainWindow = o as StageWnd;
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

            ScaleValue = (double)OnCoerceScaleValue(this, value);
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
        public StageWnd(Editor mainWnd)
        {
            InitializeComponent();
            mainWnd.Closing += MainWnd_Closing;  
        }
        private void MainWnd_Closing(object? sender, System.ComponentModel.CancelEventArgs e) => Close(); 
        private void SetBackgroundClicked(object sender, RoutedEventArgs e)
        {
            var result =  FileDialog.ImportFileDialog();
            e.Handled = true;
            
            if (result.filePath is "" or null) 
                return; 

            Bitmap image = new(result.filePath);
            
            if (image is not null)
            {
                this.image = image;
                CBit.Render(ref image, imgPreview);
            }
        }
        private async void CreateNewStageButtonPressed(object sender, RoutedEventArgs e)
        {
            if (image is null)
                await OnNoBackgroundSelected();

            List<Node> nodes = new();
            int count = nodeCtTxt.Text.ToInt();
            var name = stageNameTxt.Text.ToFileNameFormat();
            
            if (usingStarterAssets)
                StagingHost.AddPlayer(nodes);

            var stage = new Stage(name, null, nodes.ToNodeAssets());

            for (int i = 0; i < count; i++) 
                stage.create_generic_node();

            var msgResult = 
                MessageBox.Show("Stage Creation complete : Would you like to set this as the current stage and add it to the current project?", "Set Stage?", MessageBoxButton.YesNo);

            var asset = new StageAsset(stage.Name, stage);
            
            Library.Register(typeof(Stage), asset);

            if (msgResult == MessageBoxResult.Yes)
            {
                Runtime.Instance.SetStageAsset(asset);
                Runtime.Instance.AddStageToProject(asset);
            }
            Close(); 
        }

        private static async Task OnNoBackgroundSelected()
        {
            var msg = MessageBox.Show("No background selected! please navigate to a Bitmap file to continue.");
            await Task.Run(Importer.ImportAssetDialog);
        }

        private void OnStarterAssetsButtonClicked(object sender, RoutedEventArgs e) => usingStarterAssets = !usingStarterAssets;
    }
}
