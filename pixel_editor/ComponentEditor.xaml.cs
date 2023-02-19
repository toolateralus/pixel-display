using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using pixel_renderer;
using static System.Net.Mime.MediaTypeNames;
using Component = pixel_renderer.Component;

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
            data = new(component);
            AddTextBoxes(MainGrid);
        }


        public ViewerData data;
        public Grid mainGrid;
        public List<Action<string, int>> editEvents = new();
        public List<TextBox> inputFields = new();

        private void SetupViewer(Grid grid)
        {
            mainGrid = grid;
            int colCt = mainGrid.ColumnDefinitions.Count;
            int rowCt = mainGrid.RowDefinitions.Count;

            mainGrid = Inspector.GetGrid(colCt, rowCt);

            Inspector.SetRowAndColumn(mainGrid, colCt, rowCt, 0, 0);
            mainGrid.Children.Add(mainGrid);
            mainGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            mainGrid.VerticalAlignment = VerticalAlignment.Stretch;
        }
        public void AddTextBoxes(Grid viewer)
        {
            var fields = data.Fields;
            int i = 0;

            var saveBtn = Inspector.CreateButton("Save", new(0, 0, 0, 0));
            Inspector.SetRowAndColumn(saveBtn, 2, 2, 20, 20);
            viewer.Children.Add(saveBtn);

            foreach (var x in fields)
            {
                var display = Inspector.GetTextBox(x.Key);
                var input = Inspector.GetTextBox(x.Value.ToString() ?? "null");
                var button = Inspector.CreateButton("set", new(0, 0, 0, 0));


                button.Name = "edit_confirm_button_" + i.ToString();
                button.Click += ExecuteEditEvent;

                viewer.Children.Add(display);
                viewer.Children.Add(input);
                viewer.Children.Add(button);

                inputFields.Add(input);

                editEvents.Add((o, e) => SetVariable(o, e));

                Inspector.SetRowAndColumn(display, 10, 8, 0, i * 4);
                Inspector.SetRowAndColumn(input, 10, 8, 8, i * 4);
                Inspector.SetRowAndColumn(button, 3, 2, 16, i * 4);

                i++;
            }
        }
        private bool SetVariable(string o, int i)
        {
            Inspector.GetComponentRuntimeInfo(component, out var fields, out _);

            foreach (var info in fields)
                if (info.Name == o)
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(info.FieldType);
                    object value = tc.ConvertFromString(null, CultureInfo.InvariantCulture, inputFields[i].Text);
                    info.SetValue(component, value);
                    return true; 
                }
            return false;
        }
        private void ExecuteEditEvent(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                if (button.Name.ToInt() is int i)
                    if (inputFields.Count > i)
                        if (data.Fields.ElementAt(i) is KeyValuePair<string, object> kvp)
                        {
                            string field = inputFields.ElementAt(i).Text;
                            editEvents[i].Invoke(kvp.Key, i);
                        }
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

      
}