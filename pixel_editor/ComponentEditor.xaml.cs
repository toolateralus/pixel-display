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
using System.Reflection;
using System.Threading.Tasks;

namespace pixel_editor
{
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
        string EditorKey => component.Name + component.GetType().Name;
        public Component component;
        public EditorData data;
        public Grid mainGrid;
        public List<Action<string, int>> editEvents = new();
        public List<TextBox> inputFields = new();

        public ComponentEditor (Editor mainWnd, Component component)
        {
            InitializeComponent();
            this.component = component;
            mainWnd.RegisterComponentEditor(EditorKey, this);
            Closed += ComponentEditor_Closed;
            CompositionTarget.Rendering += Update;
            RegisterAction(delegate { Keyboard.ClearFocus(); }, Key.Escape);
            Refresh(component);
        }
        public void Refresh(Component component)
        {
            MainGrid.Children.Clear();
            inputFields.Clear();
            editEvents.Clear();
            data = new(component);
            AddTextBoxes(MainGrid);
        }
        private void ComponentEditor_Closed(object? sender, EventArgs e)
        {
            Editor.Current.OnEditorClosed?.Invoke(EditorKey, this);
        }

        private void Update(object? sender, EventArgs e)
        {


        }
        public void AddTextBoxes(Grid viewer)
        {
            int i = 0;
            i = SerializeFields(viewer, i);
            i = SerializeMethods(viewer, i);
        }
        private int SerializeMethods(Grid viewer, int i)
        {
            foreach (var method in data.Methods)
            {
                AddToEditor(viewer, i, "Method", method.Name);
                var button = Inspector.GetButton("Call", new(0, 0, 0, 0));
                viewer.Children.Add(button);
                Inspector.SetControlColors(button, Brushes.Red, Brushes.Black);
                Inspector.SetRowAndColumn(button, 1, 2, 10, i++);
                button.FontSize = 3;
                button.Click += delegate { method.Invoke(component, null); };
            }

            return i;
        }
        private int SerializeFields(Grid viewer, int i)
        {
            var fields = data.Fields;
            foreach (var field in fields)
            {
                string name = field.Name;
                if (field.FieldType.BaseType == typeof(Component))
                {
                    AddToEditor(viewer, i, "open editor for more options.", name);
                    var button = Inspector.GetButton("Open Editor", new(0, 0, 0, 0));
                    viewer.Children.Add(button);
                    Inspector.SetControlColors(button, Brushes.Red, Brushes.Black);
                    Inspector.SetRowAndColumn(button, 1, 3, 12, i++);
                    button.FontSize = 3;
                    button.Click += delegate 
                    {
                        ComponentEditor editor = new(Editor.Current, (Component)field.GetValue(component));
                        editor.Show();
                    };
                    i++; 
                    return i; 
                }
                if (field.FieldType == typeof(string[]))
                {
                    object obj = field.GetValue(component);
                    if (obj is null)
                        continue; 
                    string[] strings = (string[])obj;
                    var grid = Inspector.GetGrid(8, 10, 12, 16);
                    foreach (var str in strings)
                    {
                        var txtBox = Inspector.GetTextBox(str);
                        txtBox.FontSize = 4; 
                    }
                    viewer.Children.Add(grid);
                    Inspector.SetRowAndColumn(grid, 0, 0, 16, 16);
                }

                string? valStr;
                if (field != null)
                    valStr = field.GetValue(component)?.ToString();
                else valStr = "null";
                AddToEditor(viewer, i, valStr, name);
                i++;
            }

            return i;
        }
        private void AddToEditor(Grid viewer, int index, string? valStr, string name)
        {
            var nameDisplay = Inspector.GetTextBox(name);
            var inputBox = Inspector.GetTextBox(valStr);
            Inspector.SetControlColors(nameDisplay, Brushes.DarkSlateGray, Brushes.White);
            Inspector.SetControlColors(inputBox, Brushes.DarkSlateGray, Brushes.White);
            inputBox.IsReadOnly = false;
            inputBox.GotKeyboardFocus += Input_GotKeyboardFocus;
            inputBox.LostKeyboardFocus += Input_LostKeyboardFocus;
            inputBox.KeyDown += InputBox_KeyDown;
            viewer.Children.Add(nameDisplay);
            viewer.Children.Add(inputBox);
            inputFields.Add(inputBox);
            editEvents.Add((o, e) => SetVariable(o, e));
            Inspector.SetRowAndColumn(nameDisplay, 1, 8, 0, index);
            Inspector.SetRowAndColumn(inputBox, 1, 8, 8, index);
        }
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box)
                return;
            if (e.Key == Key.Return)
                Keyboard.ClearFocus();
        }
        private void Input_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box) return;
            Inspector.SetControlColors(box, Brushes.DarkSlateGray, Brushes.Black);
            for (int i = 0; i < data.Fields.Count; ++i)
                ExecuteEditEvent(i);
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
                            Runtime.Log($"Field {info.Name} of object {component.Name} -> new {info.FieldType}");
                            info.SetValue(component, obj);
                        }

                    return true; 
                }
            return false;
        }
        private void ExecuteEditEvent(int index)
        {
            if (inputFields.Count > index)
                if (data.Fields.ElementAt(index) is var field)
                {
                    Action<string, int> action = editEvents[index];
                    string name = field.Name;
                    action.Invoke(name, index);
                }
        }
        private void MainWnd_Closing(object? sender, System.ComponentModel.CancelEventArgs e) => Close(); 
    }
    public record EditorData
    {
        public List<FieldInfo> Fields = new();
        public List<MethodInfo> Methods = new();
        public EditorData(Component component)
        {
            init_data(component);

            void init_data(Component component)
            {
                Fields = Inspector.GetSerializedFields(component).ToList();
                Methods = Inspector.GetSerializedMethods(component).ToList();
            }
        }
    }
   ;
}