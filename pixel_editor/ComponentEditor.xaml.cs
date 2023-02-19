using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static pixel_renderer.Input;
using pixel_renderer;
using Component = pixel_renderer.Component;
using System.Windows.Input;

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
            CompositionTarget.Rendering += Update;
            RegisterAction(delegate { Keyboard.ClearFocus(); }, Key.Escape);
        }

        private void Update(object? sender, EventArgs e)
        {


        }

        public ViewerData data;
        public Grid mainGrid;
        public List<Action<string, int>> editEvents = new();
        public List<TextBox> inputFields = new();

       
        public void AddTextBoxes(Grid viewer)
        {
            var fields = data.Fields;
            int i = 0;

          

            foreach (var x in fields)
            {
                var display = Inspector.GetTextBox(x.Key);
                string valStr;
                
                if (x.Value != null)
                {
                    if (x.Value is Vec2 vec)
                        valStr = vec.AsString();
                   
                    else valStr = x.Value.ToString();
                }
                else valStr = "null";

                var input = Inspector.GetTextBox(valStr);
                
                var button = Inspector.CreateButton("set", new(0, 0, 0, 0));

                Inspector.SetControlColors(display, Brushes.DarkSlateGray, Brushes.White);
                Inspector.SetControlColors(input, Brushes.DarkSlateGray, Brushes.White);
                Inspector.SetControlColors(button, Brushes.SlateGray, Brushes.White);


                input.IsReadOnly = false;
                input.GotKeyboardFocus += Input_GotKeyboardFocus;
                input.LostKeyboardFocus += Input_LostKeyboardFocus;

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
            var saveBtn = Inspector.CreateButton("Save", new(0, 0, 0, 0));

            viewer.Children.Add(saveBtn);
            saveBtn.Click += SaveBtn_Click;

            Inspector.SetRowAndColumn(saveBtn, 1,1, 16,16);

        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; 
            int i = 0; 
            foreach (var field in data.Fields)
            {
                editEvents[i].Invoke(field.Key, i);
                i++;
            }
        }

        private void Input_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box) return;
            Inspector.SetControlColors(box, Brushes.DarkSlateGray, Brushes.White);
        }

        private void Input_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box) return;
            Inspector.SetControlColors(box, Brushes.Black, Brushes.DarkSlateGray);
        }

        private bool SetVariable(string o, int i)
        {
            Inspector.GetComponentRuntimeInfo(component, out var fields, out _);

            foreach (var info in fields)
                if (info.Name == o)
                {
                    bool gotValue = CommandParser.TryParse(inputFields[i].Text, out List<object> results);

                    foreach(var obj in results)
                        if (obj.GetType() == info.FieldType)
                        {
                            Runtime.Log($"ComponentEditor field was successfully parsed back into an object of type {info.FieldType}");
                            info.SetValue(component, obj);
                        }

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