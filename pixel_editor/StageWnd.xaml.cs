﻿using pixel_renderer;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        Bitmap? background;

        public StageWnd()
        {
            InitializeComponent();
        }
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

        private void SetBackgroundClicked(object sender, RoutedEventArgs e)
        {

            Importer.ImportFileDialog(out Asset result);

            if (result is null) return;

            if (result as BitmapAsset is null) return;

            var bmpAsset = result as BitmapAsset;

            var image = bmpAsset.RuntimeValue ?? new(256,256); 

            if (image is not null)
            {
                background = image;
                CBit.Render(ref image, imgPreview);
            }
        }
        private async void CreateNewStageButtonPressed(object sender, RoutedEventArgs e)
        {
            int count = nodeCtTxt.Text.ToInt();
            var name = stageNameTxt.Text.ToFileNameFormat();

            List<Node> nodes = new();
            for (int i = 0; i < count; i++)
                Node.CreateGenericNode(nodes, i);

            if (usingStarterAssets) Staging.AddPlayer(nodes);
            if (background is null)
            {
                var msg = MessageBox.Show("No background selected! please navigate to a Bitmap file to continue.");
                await Task.Run(Importer.ImportAssetDialog);
                if (background is null) return;
            }
            var stage = new Stage(name, background, nodes);

            var msgResult = MessageBox.Show("Stage Creation complete : Would you like to set this as the current stage?", "Set Stage?", MessageBoxButton.YesNo);

            if (msgResult == MessageBoxResult.Yes)
            {

                var asset = new StageAsset("", Stage.New);
                if(stage is not null)
                    if(stage.Nodes is not null)
                        asset = new StageAsset(stage.Name, stage);

                Library.Register(typeof(Stage), asset);
                Staging.SetCurrentStage(asset);
            }
        }
        private void OnStarterAssetsButtonClicked(object sender, RoutedEventArgs e) => usingStarterAssets = !usingStarterAssets;
    }
}

public class InspectorProperty
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    //public static List<InspectorProperty> GetForNode(Node node)
    //{
    //    List<InspectorProperty> properties = new();
    //    foreach (var component in node.ComponentsList)
    //    {
    // deserialize component info
    //    }
    //}
}
