using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Pixel.Input;
using Pixel;
using Component = Pixel.Types.Components.Component;
using System.Windows.Input;
using System.Reflection;
using Pixel_Editor;
using Console = Pixel_Editor.Console;
using PixelLang;
using PixelLang.Tools;

namespace Pixel_Editor.Source
{
    public class ComponentEditor
    {
        public Component component;
        public bool Disposing { get; internal set; }
        public ComponentEditorData data;
        public ItemsControl memberStackPanel = new();
        public List<Action<string, int>> editEvents = new();
        public List<TextBox> inputFields = new();
        public void Refresh(Component component)
        {
            this.component = component;

            memberStackPanel = Editor.Current.componentEditorControl;
            memberStackPanel.Items.Clear();

            if (component is null)
                return;

            data = new(component);
            AddTextBoxes();
        }
        private void SerializeMethods()
        {
            foreach (var method in data.Methods)
            {
                var button = Inspector.GetButton($"{method.Name}();", new(0, 0, 0, 0));
                button.HorizontalAlignment = HorizontalAlignment.Stretch;
                button.Click += delegate
                {
                    try
                    {
                        method.Invoke(component, null);
                    }
                    catch (Exception ex)
                    {
                        Runtime.Log(ex.Message);
                        return;
                    }
                };
                var control = AddContentToList(method.Name);
                control.Content = button;
            }
        }
        private void SerializeFields()
        {
            var fields = data.Fields;
            NewData();

            foreach (var field in fields)
            {
                if (field.FieldType.BaseType == typeof(Component))
                {
                    AddNestedComponentEditorButton(field);
                    return;
                }
                if (field.FieldType == typeof(bool))
                {
                    AddBoolCheckBox(field);
                    return;
                }
                foreach (var attr in field.GetCustomAttributes())
                    if (attr.GetType() == typeof(InputFieldAttribute))
                    {
                        AddInputFieldTextBox(field);
                        return;
                    }
                if (field.FieldType == typeof(string[]))
                {
                    AddStringArrayTextBox(field);
                    return;
                }
                AddOtherTextBox(field);
            }
        }

        private ContentControl AddContentToList(string label)
        {
            Grid memberGrid = new();
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            TextBox box = Inspector.GetTextBox(label);
            Grid.SetColumn(box, 0);
            memberGrid.Children.Add(box);
            
            ContentControl control = new();
            Grid.SetColumn(control, 1);
            memberGrid.Children.Add(control);

            memberStackPanel.Items.Add(memberGrid);
            return control;
        }

        private void AddInputFieldTextBox(FieldInfo field)
        {
            if (field.GetValue(component) is not string currentvalue)
                throw new InvalidCastException();
            var textbox = new TextBox
            {
                Text = currentvalue,
                Name = field.Name,
                AcceptsReturn = true,
                AcceptsTab = true,
            };
            textbox.LostFocus += (e, o) =>
            {
                if (o.RoutedEvent != null)
                    o.Handled = true;

                if (e is not TextBox tb)
                    return;

                field.SetValue(component, tb.Text);

                UpdateData();
            };
            var control = AddContentToList(field.Name);
            control.Content = textbox;
        }
        private void AddStringArrayTextBox(FieldInfo field)
        {
            if (field.GetValue(component) is not string[] array)
                throw new InvalidCastException();
            for (int i = 0; i < array.Length; i++)
            {
                string? str = array[i];
                var txtBox = Inspector.GetTextBox(str);
                txtBox.IsReadOnly = false;
                txtBox.KeyDown += (s, e) => TxtBox_KeyDown(s, e, field, component, array);
                txtBox.LostKeyboardFocus += Input_LostKeyboardFocus;
                txtBox.GotKeyboardFocus += Input_GotKeyboardFocus;
                var control = AddContentToList($"{field.Name}:{i}");
                control.Content = txtBox;
            }
        }

        private void AddBoolCheckBox(FieldInfo field)
        {
            if (field.GetValue(component) is not bool curVal)
                throw new InvalidCastException();
            CheckBox checkBox = new()
            {
                Content = curVal.ToString(),
                Name = field.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            checkBox.Click += (e, o) =>
            {
                o.Handled = true;
                if (e is not CheckBox cb)
                    return;
                if (cb.IsChecked is bool val)
                    field.SetValue(component, val);
                UpdateData();
            };
            var control = AddContentToList(field.Name);
            control.Content = checkBox;
        }
        private void NewData()
        {
            if (component != null && data.Component.TryGetTarget(out var dataComp) && dataComp != component)
                data = new(component);
        }
        #region "Add" Button/TextBox/Grid methods
        private void AddTextBoxes()
        {
            SerializeFields();
            SerializeMethods();
        }
        private void AddNestedComponentEditorButton(FieldInfo field)
        {
            var button = Inspector.GetButton("Open Editor", new(0, 0, 0, 0));
            Inspector.SetControlColors(button, Brushes.Red, Brushes.Black);
            button.Click += delegate
            {
                if (field.GetValue(component) is not Component Component)
                {
                    Pixel_Editor.Console.Error("Cannot edit a null component.");
                    return;
                }
                Refresh(Component);
            };
            var control = AddContentToList(field.Name);
            control.Content = button;
        }
        private void AddOtherTextBox(FieldInfo field)
        {
            if (field.GetValue(component)?.ToString() is not string valStr ||
                Inspector.GetTextBox(valStr) is not TextBox textbox)
                return;
            Inspector.SetControlColors(textbox, Brushes.DarkSlateGray, Brushes.White);
            textbox.IsReadOnly = false;
            textbox.GotKeyboardFocus += Input_GotKeyboardFocus;
            textbox.LostKeyboardFocus += Input_LostKeyboardFocus;
            textbox.KeyDown += InputBox_KeyDown;
            inputFields.Add(textbox);
            editEvents.Add((o, e) => SetVariable(o, e));
            var control = AddContentToList(field.Name);
            control.Content = textbox;
        }
        #endregion
        private bool SetVariable(string o, int i)
        {
            Inspector.GetComponentRuntimeInfo(component, out var fields, out _);

            foreach (var info in fields)
                if (info.Name == o)
                {
                    InputProcessor.TryParse(inputFields[i].Text, out List<object> results);
                    foreach (var obj in results)
                        if (obj.GetType() == info.FieldType)
                        {
                            Runtime.Log($"Field {info.Name} of object {component.Name} -> new {info.FieldType} of value {obj}");
                            info.SetValue(component, obj);
                        }

                    Refresh(component);
                    return true;
                }

            Refresh(component);
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
                    component?.OnFieldEdited(name);
                }
        }
        internal void Dispose()
        {
            Disposing = true;
            data = null;
            inputFields?.Clear();
            editEvents?.Clear();
            Disposing = false;
        }
        #region WPF Events

        private void TxtBox_KeyDown(object sender, KeyEventArgs e, FieldInfo field, Component component, string[] strings)
        {
            if (sender is TextBox box)
            {
                var index = box.Name.ToInt();
                var txt = box.Text;
                if (e.Key == Key.Return)
                {
                    if (strings.Length > index)
                        strings[index] = txt;
                    field.SetValue(component, strings);
                    Keyboard.ClearFocus();
                    UpdateData();
                }
            }
        }
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box)
                return;
            if (e.Key == Key.Return)
            {
                UpdateData();
                Keyboard.ClearFocus();
            }
        }
        private void Input_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box || data is null)
                return;
            Inspector.SetControlColors(box, Brushes.DarkSlateGray, Brushes.Black);
            for (int i = 0; i < data.Fields.Count; ++i)
                ExecuteEditEvent(i);
            UpdateData();
            Refresh(component);
        }
        internal void UpdateData()
        {
            if (data is null
            || data.Fields is null
            || data.Fields.Count == 0)
                return;

            for (int i = 0; i < data.Values.Count; i++)
                if (data.HasValueChanged(i, data.Values[i], out var newVal))
                {
                    data.Values[i] = newVal;
                }
        }

        private void Input_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box) return;
            Inspector.SetControlColors(box, Brushes.White, Brushes.DarkSlateGray);
            UpdateData();

        }

        #endregion
    }
    public record ComponentEditorData
    {
        public readonly IReadOnlyCollection<FieldInfo> Fields;
        public readonly IReadOnlyCollection<MethodInfo> Methods;
        public readonly List<object> Values;
        public readonly WeakReference<Component> Component;

        public ComponentEditorData(Component component)
        {
            Component = new(component);
            Fields = Inspector.GetSerializedFields(component).ToList();
            Methods = Inspector.GetSerializedMethods(component).ToList();

            var values = new object[Fields.Count];

            for (int i = 0; i < Fields.Count; i++)
            {
                FieldInfo? field = Fields.ElementAt(i);
                values[i] = GetValueAtIndex(i);
            }

            Values = values.ToList();
        }
        public IReadOnlyCollection<object> GetAllValues(out int count)
        {
            count = Values.Count;
            return Values;
        }
        public object? GetValueAtIndex(int index)
        {
            if (IsReferenceAlive(out var component))
                return Fields.ElementAt(index)?.GetValue(component);
            return false;
        }
        public void UpdateChangedValues(object[] data)
        {
            if (data.Length != Values.Count)
            {
                Runtime.Log("component update invalidated : input array was the wrong size.");
                return;
            }

            for (int i = 0; i < Values.Count; ++i)
            {
                object localVal = Values.ElementAt(i);
                object newVal = data.ElementAt(i);

                if (localVal == newVal)
                    continue;

                Values[i] = newVal;
            }
        }
        public bool SetValueAtIndex(int index, object value)
        {
            if (IsReferenceAlive(out var component))
            {
                Fields.ElementAt(index).SetValue(component, value);
                return true;
            }
            return false;
        }
        public bool IsReferenceAlive(out Component component)
        {
            if (!Component.TryGetTarget(out component))
                return false;
            return true;
        }
        public bool HasValueChanged(int index, object value, out object newValue)
        {
            newValue = GetValueAtIndex(index);
            if (newValue == value)
                return true;
            return false;
        }

    }
}