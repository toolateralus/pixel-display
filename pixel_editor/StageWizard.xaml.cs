using Pixel;
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Drawing;
using System.IO;
using System.Windows;


namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for StageWnd.xaml
    /// </summary>
    public partial class StageWnd : Window
    {
        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(StageWnd), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));
        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            return o is StageWnd mainWindow ? mainWindow.OnCoerceScaleValue((double)value) : value;
        }
        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            StageWnd mainWindow = o as StageWnd;
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
        public Action OnCreated { get; private set; }
        #endregion

        bool usingStarterAssets = false;
        Metadata background_meta = Stage.DefaultBackgroundMetadata;
        
        public StageWnd(Editor mainWnd)
        {
            InitializeComponent();
            if (File.Exists(background_meta.Path))
                CBit.Render(new Bitmap(background_meta.Path), imgPreview);
            mainWnd.Closing += MainWnd_Closing;
            OnCreated += Close;
        }

        private void MainWnd_Closing(object? sender, System.ComponentModel.CancelEventArgs e) => Close(); 
        private void SetBackgroundClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            background_meta = FileDialog.ImportFileDialog();

            if (background_meta.Path is "" or null)
                background_meta = Stage.DefaultBackgroundMetadata;

            if (File.Exists(background_meta.Path))
                CBit.Render(new Bitmap(background_meta.Path), imgPreview);
        }
        private void CreateNewStageButtonPressed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            int count = nodeCtTxt.Text.ToInt();
            var name = stageNameTxt.Text.ToFileNameFormat();
            
            var stage = StagingHost.Standard();

            for (int i = 0; i < count; i++) 
                stage.AddNode(Rigidbody.Standard());

            var meta = new Metadata(Constants.WorkingRoot + Constants.AssetsDir + "\\" + stage.Name + Constants.AssetsExt);


            Library.Register(meta, stage);

            var msgResult = MessageBox.Show(
                "Stage Creation complete : Would you like to set this as the current stage and add it to the current project?",
                "Set Stage?", MessageBoxButton.YesNo);

            if (msgResult == MessageBoxResult.Yes)
            {
                Runtime.Current.SetStage(stage);
                Runtime.Current.project.AddStage(stage);
            }
            OnCreated?.Invoke(); 
        }
        private void OnStarterAssetsButtonClicked(object sender, RoutedEventArgs e) => usingStarterAssets = !usingStarterAssets;
    }
}
