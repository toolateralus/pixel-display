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
                string valStr;
                
                if (x.Value != null)
                {
                    if (x.Value is Vec2 vec)
                        valStr = vec.AsString();
                    else valStr = x.Value.ToString();
                }
                else valStr = "null";

                string name = x.Key;
                var nameDisplay = Inspector.GetTextBox(name);
                var inputBox = Inspector.GetTextBox(valStr);

                Inspector.SetControlColors(nameDisplay, Brushes.DarkSlateGray, Brushes.White);
                Inspector.SetControlColors(inputBox, Brushes.DarkSlateGray, Brushes.White);

                inputBox.IsReadOnly = false;
                inputBox.GotKeyboardFocus += Input_GotKeyboardFocus;
                inputBox.LostKeyboardFocus += Input_LostKeyboardFocus;

                viewer.Children.Add(nameDisplay);
                viewer.Children.Add(inputBox);
                inputFields.Add(inputBox);
                editEvents.Add((o, e) => SetVariable(o, e));
                Inspector.SetRowAndColumn(nameDisplay, 1, 8, 0, i);
                Inspector.SetRowAndColumn(inputBox, 1, 8, 8, i);

                i++;
            }

            var saveBtn = Inspector.GetButton("Save", new(0, 0, 0, 0));
            viewer.Children.Add(saveBtn);
            saveBtn.Click += SaveBtn_Click;
            saveBtn.FontSize = 2; 
            Inspector.SetRowAndColumn(saveBtn, 1, 2, 0, 18);

         
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; 
            for(int i = 0; i < data.Fields.Count; ++i)
                ExecuteEditEvent(i);
        }

        private void Input_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box) return;
            Inspector.SetControlColors(box, Brushes.DarkSlateGray, Brushes.Black);
        }

        private void Input_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box) return;
            Inspector.SetControlColors(box, Brushes.White, Brushes.DarkSlateGray);
        }

        private bool SetVariable(string o, int i)
        {
            Inspector.GetComponentRuntimeInfo(component, out var fields, out _);

            foreach (var info in fields)
                if (info.Name == o)
                {
                    CommandParser.TryParse(inputFields[i].Text, out List<object> results);

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
        private void ExecuteEditEvent(int index)
        {
            if (inputFields.Count > index)
                if (data.Fields.ElementAt(index) is KeyValuePair<string, object> kvp)
                {
                    string field = inputFields.ElementAt(index).Text;
                    Action<string, int> action = editEvents[index];
                    string name = kvp.Key;
                    action.Invoke(name, index);
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