using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq; 
using System.Windows.Controls;
using PixelRenderer; 
using PixelRenderer.Components;
using System.Timers;
using System.Windows.Media;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes; 
namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Main : Window
    {
        EngineInstance engine; 
        Inspector inspector;
        private void OnDisable(object? sender, EventArgs e)
        {
            engine.Close();
        }
        public Main()
        {
            InitializeComponent();
            engine = new();
            engine.Show();
            Closing += OnDisable; 
            inspector = new Inspector(inspectorObjName, inspectorObjInfo, inspectorChildGrid);
            CompositionTarget.Rendering += Update;

        }

        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);
            if (!Runtime.Instance.running 
                || Rendering.State != RenderState.Scene) return; 

            Rendering.Render(image); 
        }
        #region ScaleValue Depdency Property
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(Main), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            Main mainWindow = o as Main;
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Main mainWindow = o as Main;
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

        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        #endregion
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
        private static int renderStateIndex = 0; 
        private void OnViewChanged(object sender, RoutedEventArgs e)
        {
            renderStateIndex++; 
            if (renderStateIndex >= sizeof(RenderState))
            {
                renderStateIndex = 0; 
            }
            Rendering.State = (RenderState)renderStateIndex;
        }
    }
    public class Inspector
    {
        public Label name;
        public Label objInfo;
        public Grid componentGrid;
        public Node node;
        public Dictionary<Type, Component> components = new(); 
        
        public Inspector(Label name, Label objInfo, Grid componentGrid)
        {
            this.name = name;
            this.objInfo = objInfo;
            this.componentGrid = componentGrid;
            name.Content = "Name Gotten";
            objInfo.Content = "Object Gotten";
        }

       

        Stage stage; 
        public void Update(object? sender, EventArgs e)
        {
            UpdateInspector(); 
            if (stage == null && Runtime.Instance.stage != null)
            {
                stage = Runtime.Instance.stage;
                node = stage.FindNode("Player");
            }
        }
        int i = 0; 
        private void UpdateInspector()
        {
            i++;
            if (i < 2500) return;
            if (node == null) return;
            i = 0; 
            UpdateCurrentSelectedComponentInfo();
        }

        private void UpdateCurrentSelectedComponentInfo()
        {
            name.Content = node.Name;
            objInfo.Content = $"{node.Components.Count} Components";
            components = node.Components;

            int i = 0;
            var thickness = new Thickness(0, 0, 0, 0);

            foreach (var value in components.Keys)
            {
                Label nameLabel = CreateInspectorComponentLabel(thickness, value);
                SetInspectorComponentLabelPosition(i, nameLabel);
                componentGrid.Children.Add(nameLabel);
                componentGrid.UpdateLayout();
                i++;
            }
        }

        private static void SetInspectorComponentLabelPosition(int i, Label nameLabel)
        {
            nameLabel.SetValue(Grid.RowSpanProperty, 1);
            nameLabel.SetValue(Grid.ColumnSpanProperty, 2);
            nameLabel.SetValue(Grid.RowProperty, i);
        }
        private static Label CreateInspectorComponentLabel(Thickness margin, Type typeName)
        {
            return new Label
            {
                Content = typeName.Name,
                FontSize = 5f,
                Background = Brushes.DarkGray,
                Margin = margin,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
        }
    }
}
