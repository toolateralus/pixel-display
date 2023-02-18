using System;
using System.Windows;
using pixel_renderer;

namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for StageWnd.xaml
    /// </summary>
    public partial class ComponentEditor : Window
    {
        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(ComponentEditor), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            return o is ComponentEditor mainWindow ? mainWindow.OnCoerceScaleValue((double)value) : value;
        }
        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ComponentEditor mainWindow = o as ComponentEditor;
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
        #endregion

        public Component component;

        public ComponentEditor (Editor mainWnd, Component component)
        {
            InitializeComponent();
            this.component = component;
            mainWnd.componentEditor = this;
        }
        private void MainWnd_Closing(object? sender, System.ComponentModel.CancelEventArgs e) => Close(); 
    }
}
