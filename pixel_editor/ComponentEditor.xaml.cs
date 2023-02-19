using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
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
        public ComponentViewer viewer;
        public ComponentEditor (Editor mainWnd, Component component)
        {
            InitializeComponent();
            this.component = component;
            mainWnd.componentEditor = this;
            viewer = new ComponentViewer(component, MainGrid);
        }
        
        private void MainWnd_Closing(object? sender, System.ComponentModel.CancelEventArgs e) => Close(); 
    }

    public record ViewerData
    {
        public ViewerData(Component component)
        {
            init_fields(component);

            void init_fields(Component component)
            {
                var fields = component.GetSerializedFields();
                foreach (var field in fields)
                    Fields.Add(field.Name, field.GetValue(component));
            }
        }
        public Dictionary<string, object> Fields { get; private set; } = new();
    }

    public class ComponentViewer
    {
        public ViewerData data;
        public Grid viewerGrid;
        public Grid mainGrid;
        public List<Action<string, object>> FinalizeEditEvents = new();


        public ComponentViewer(Component component, Grid mainGrid)
        {
            data = new(component);
            this.mainGrid = mainGrid;
            viewerGrid = mainGrid;
            AddTextBoxes(viewerGrid);
        }

        private void SetupViewer(Grid grid)
        {
            mainGrid = grid;
            int colCt = mainGrid.ColumnDefinitions.Count;
            int rowCt = mainGrid.RowDefinitions.Count;

            viewerGrid = Inspector.GetGrid(colCt, rowCt);

            Inspector.SetRowAndColumn(viewerGrid, colCt, rowCt, 0, 0);
            mainGrid.Children.Add(viewerGrid);
            viewerGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            viewerGrid.VerticalAlignment = VerticalAlignment.Stretch;
        }

        public void AddTextBoxes(Grid viewer)
        {
            var fields = data.Fields;
            int i = 0;

            var saveBtn = Inspector.CreateButton("Save", new(0,0,0,0));
            Inspector.SetRowAndColumn(saveBtn, 2, 2, 20, 20);
            viewer.Children.Add(saveBtn);

            foreach (var x in fields)
            {
                var display = Inspector.GetTextBox(x.Key);
                var input = Inspector.GetTextBox(x.Value.ToString() ?? "null");
                var button = Inspector.CreateButton("set", new(0,0,0,0));


                button.Name = "edit_confirm_button_" + i.ToString();
                button.Click += ExecuteEditEvent; 

                viewer.Children.Add(display);
                viewer.Children.Add(input);
                viewer.Children.Add(button);

                FinalizeEditEvents.Add((o, e) => SetVariable(o, e));
                
                Inspector.SetRowAndColumn(display, 10, 8, 0 , i * 4);
                Inspector.SetRowAndColumn(input, 10, 8, 8 , i * 4);
                Inspector.SetRowAndColumn(button, 3, 2, 16 , i * 4);
                
                i++;
            }
        }

        private bool SetVariable(string o, object e)
        {
            if (data.Fields.ContainsKey(o))
            {
                data.Fields[o] = e;
                return true;
            }
            return false; 
        }

        private void ExecuteEditEvent(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                if (button.Name.ToInt() is int i)
                    if(data.Fields.ElementAt(i) is KeyValuePair<string, object> kvp)
                        FinalizeEditEvents[i].Invoke(kvp.Key, kvp.Value);
        }

      
    }
}