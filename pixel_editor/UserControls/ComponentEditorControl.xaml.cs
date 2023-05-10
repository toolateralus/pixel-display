using Pixel;
using PixelLang.Tools;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pixel_Editor.Source;
using Component = Pixel.Types.Components.Component;
using System.Windows.Media;

namespace Pixel_Editor
{
    /// <summary>
    /// Interaction logic for ComponentEditorControl.xaml
    /// </summary>
    public partial class ComponentEditorControl : UserControl
    {
        public ObservableCollection<MemberEditor> Members { get; private set; } = new();
        public ComponentEditorControl()
        {
            InitializeComponent();
        }
        public Component component;
        public bool Disposing { get; internal set; }
        public ComponentEditorData data;
        public void Refresh(Component component)
        {
            this.component = component;
            Members.Clear();

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
                MemberEditor member = new(button, method.Name);
                Members.Add(member);
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
                    continue;
                }
                if (field.FieldType == typeof(bool))
                {
                    AddBoolCheckBox(field);
                    continue;
                }
                if (field.FieldType == typeof(string) &&
                    field.GetCustomAttribute<InputFieldAttribute>() is not null)
                {
                    AddInputFieldTextBox(field);
                    continue;
                }
                if (field.FieldType == typeof(string[]))
                {
                    AddStringArrayTextBox(field);
                    continue;
                }
                AddOtherTextBox(field);
            }
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
            textbox.LostFocus += (s, e) =>
            {
                if (e.RoutedEvent != null)
                    e.Handled = true;
                if (s is not TextBox tb)
                    return;
                field.SetValue(component, tb.Text);
                UpdateData();
            };
            textbox.GotKeyboardFocus += TextBoxGotKeyboardFocus;
            MemberEditor member = new(textbox, field.Name);
            Members.Add(member);
        }
        private void AddStringArrayTextBox(FieldInfo field)
        {
            if (field.GetValue(component) is not string[] array)
                throw new InvalidCastException();
            for (int i = 0; i < array.Length; i++)
            {
                int index = i;
                var txtBox = Inspector.GetTextBox(array[i]);
                txtBox.IsReadOnly = false;
                txtBox.KeyDown += InputBox_KeyDown;
                txtBox.GotKeyboardFocus += TextBoxGotKeyboardFocus;
                txtBox.LostKeyboardFocus += (s, e) =>
                {
                    e.Handled = true;
                    Inspector.SetControlColors(txtBox, Brushes.DarkSlateGray, Brushes.Black);
                    array[index] = txtBox.Text;
                    component?.OnFieldEdited(field.Name);
                    UpdateData();
                    Refresh(component);
                };
                MemberEditor member = new(txtBox, $"{field.Name}:{i}");
                Members.Add(member);
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
            MemberEditor member = new(checkBox, field.Name);
            Members.Add(member);
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
            MemberEditor member = new(button, field.Name);
            Members.Add(member);
        }
        private void AddOtherTextBox(FieldInfo field)
        {
            if (field.GetValue(component)?.ToString() is not string valStr ||
                Inspector.GetTextBox(valStr) is not TextBox textbox)
                return;
            Inspector.SetControlColors(textbox, Brushes.DarkSlateGray, Brushes.White);
            textbox.IsReadOnly = false;
            textbox.GotKeyboardFocus += TextBoxGotKeyboardFocus;
            textbox.LostKeyboardFocus += (s, e) => UpdateField(field, textbox, e);
            textbox.KeyDown += InputBox_KeyDown;
            MemberEditor member = new(textbox, field.Name);
            Members.Add(member);
        }
        #endregion
        internal void Dispose()
        {
            Disposing = true;
            data = null;
            Disposing = false;
        }
        #region WPF Events

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox box)
                return;
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                UpdateData();
                Keyboard.ClearFocus();
            }
        }
        private void UpdateField(FieldInfo field, TextBox box, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
            Inspector.SetControlColors(box, Brushes.DarkSlateGray, Brushes.Black);
            InputProcessor.TryParse(box.Text, out List<object> results);
            foreach (var obj in results)
                if (obj.GetType() == field.FieldType)
                {
                    Runtime.Log($"Field {field.Name} of object {component.Name} -> new {field.FieldType} of value {obj}");
                    field.SetValue(component, obj);
                }
            Refresh(component);
            component?.OnFieldEdited(field.Name);
            UpdateData();
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

        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox box)
                return;
            e.Handled = true;
            Inspector.SetControlColors(box, Brushes.White, Brushes.DarkSlateGray);
            UpdateData();
        }

            #endregion
    }
    public class MemberEditor : INotifyPropertyChanged
    {
        private string name;
        private Control inputControl;

        public MemberEditor(Control inputControl, string name)
        {
            this.inputControl = inputControl;
            this.name = name;
            PropertyChanged?.Invoke(this, new(nameof(InputControl)));
            PropertyChanged?.Invoke(this, new(nameof(Name)));
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new(nameof(Name)));
            }
        }
        public Control InputControl
        {
            get => inputControl;
            set
            {
                inputControl = value;
                PropertyChanged?.Invoke(this, new(nameof(InputControl)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
