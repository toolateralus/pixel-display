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
using System.Reflection; 
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
        public static readonly DependencyProperty ScaleValueProperty = 
            DependencyProperty.Register("ScaleValue", typeof(double), typeof(Main),
                new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged),
                    new CoerceValueCallback(OnCoerceScaleValue)));
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
            viewBtn.Content = Rendering.State.ToString(); 
        }
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            if (!Runtime.Instance.running)
            {
                engine.Accept_Clicked(sender, e); 
            }
        }
    }
    public class Inspector
    {
        private Label name;
        private Label objInfo;
        private Grid componentGrid;
        private Node node;
        private Dictionary<Type, Component> components = new(); 
        private Stage stage; 
        public Inspector(Label name, Label objInfo, Grid componentGrid)
        {
            this.name = name;
            this.objInfo = objInfo;
            this.componentGrid = componentGrid;
            name.Content = "Name Gotten";
            objInfo.Content = "Object Gotten";
        }
        

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
            if (i < 25) return;
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

            foreach (var value in components.Values)
            {
                string componentInfo = GetComponentInfo(value);
                var control = CreateBlock(componentInfo ,thickness);
                AddBlockToGrid(i, control);
                componentGrid.Children.Add(control);
                componentGrid.UpdateLayout();
                i++;
            }
        }
        private static void AddLabelToGrid(int i, Label nameLabel)
        {
            nameLabel.SetValue(Grid.RowSpanProperty, 2);
            nameLabel.SetValue(Grid.ColumnSpanProperty, 8);
            nameLabel.SetValue(Grid.RowProperty, i);
        }
        private static void AddBlockToGrid(int i, TextBlock block)
        {
            block.SetValue(Grid.RowSpanProperty, 2);
            block.SetValue(Grid.ColumnSpanProperty, 8);
            block.SetValue(Grid.RowProperty, i);
        }
        private static string GetComponentInfo(Component component)
        {
            IEnumerable<PropertyInfo> props = component.GetType().GetRuntimeProperties();
            IEnumerable<FieldInfo> fields = component.GetType().GetRuntimeFields();
            string output = "properties : \n";
            foreach (var prop in props)
            {
                output += $"{prop.Name} \n";
            }
            foreach (var field in fields)
            {
                output += $"{field} \n";
            }
            return output; 
        }
        private static Label CreateLabel(string componentInfo, Thickness margin)
        {
            margin.Bottom = componentInfo.Split(' ').Length;
            return new Label
            {
                Content = componentInfo,
                FontSize = 2.25f,
                Background = Brushes.DarkGray,
                Foreground = Brushes.White, 
                Margin = margin,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic")
            };
        }
        private static TextBlock CreateBlock(string componentInfo, Thickness margin)
        {
            margin.Bottom = componentInfo.Split(' ').Length + 3;
            return new TextBlock()
            {
                Text = componentInfo,
                FontSize = 2.25f,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic"),
                TextWrapping = TextWrapping.Wrap,
                Background = Brushes.DarkGray,
                Foreground = Brushes.White,
                
                Margin = margin,
                
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
        }
    }
}
