﻿using System;
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
using pixel_editor;

namespace Pixel_Editor.Source
{
    public class ComponentEditor
    {
        int lhs_width = 2;
        int lhs_height = 1;

        int rhs_width = 2;
        int rhs_height = 1;

        public Component component;
        public bool Disposing { get; internal set; }
        public ComponentEditorData data;
        public Grid mainGrid;
        public Grid myGrid;
        public List<Action<string, int>> editEvents = new();
        public List<TextBox> inputFields = new();
        List<UIElement> uiElements = new();

        public ComponentEditor()
        {
            GetEvents();
        }
        private void SetupComponentEditorGrid()
        {
            mainGrid = Editor.Current.inspectorGrid;
            myGrid ??= new();

            if (!mainGrid.Children.Contains(myGrid))
                mainGrid.Children.Add(myGrid);

            Inspector.SetRowAndColumn(myGrid, 1, 3, 0, 15);
        }
        private void GetEvents()
        {
            CompositionTarget.Rendering += Update;
        }
        public void Refresh(Component component)
        {
            this.component = component;

            SetupComponentEditorGrid();

            if (component is null)
                return;

            data = new(component);

            AddTextBoxes(mainGrid);
        }
        private int SerializeMethods(Grid viewer, int i)
        {
            foreach (var method in data.Methods)
            {
                AddToEditor(viewer, i, "Method", method.Name, false);
                var button = Inspector.GetButton(method.Name + "();", new(0, 0, 0, 0));
                viewer.Children.Add(button);
                uiElements.Add(button);
                Inspector.SetRowAndColumn(button, 1, 4, 22, i++ + 1);
                button.FontSize = 3;
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
            }
            return i;
        }
        private int SerializeFields(Grid viewer, int i)
        {
            var fields = data.Fields;
            NewData();

            foreach (var field in fields)
            {
                bool bothColumns = true;
                string name = field.Name;

                if (field.FieldType.BaseType == typeof(Component))
                    return AddNestedComponentEditorButton(viewer, ref i, field, name);

                if (field.FieldType == typeof(bool))
                {
                    object obj = field.GetValue(component);

                    if (obj is not bool val)
                        continue;

                    AddBoolCheckBox(viewer, ref i, field, val);
                    bothColumns = false;
                }

                foreach (var attr in field.GetCustomAttributes())
                    if (attr.GetType() == typeof(InputFieldAttribute))
                        AddInputFieldTextBox(viewer, ref i, field, name);



                if (field.FieldType == typeof(string[]))
                {
                    object obj = field.GetValue(component);

                    if (obj is not string[] strings)
                        continue;

                    int strIndex = 0;

                    foreach (var str in strings)
                        AddStringListTextBox(viewer, ref i, field, strings, ref strIndex, str);
                    bothColumns = false;
                    return i;
                }

                string? valStr;

                if (field != null)
                    valStr = field.GetValue(component)?.ToString();
                else valStr = "null";

                AddToEditor(viewer, i, valStr, name, bothColumns);
                i++;
            }
            return i;
        }

        private void AddInputFieldTextBox(Grid viewer, ref int i, FieldInfo field, string name)
        {
            string currentvalue = (string)field.GetValue(component);

            if (currentvalue is null)
                return;

            var textbox = Inspector.GetInputField(currentvalue);
            textbox.Name = $"textBox{i}";
            textbox.AcceptsReturn = true;
            textbox.AcceptsTab = true;
            textbox.FontSize = 4;
            textbox.VerticalAlignment = VerticalAlignment.Stretch;
            textbox.HorizontalAlignment = HorizontalAlignment.Stretch;
            textbox.LostFocus += (e, o) =>
            {
                if (o.RoutedEvent != null)
                    o.Handled = true;

                if (e is not TextBox tb)
                    return;

                field.SetValue(component, tb.Text);

                UpdateData();
            };
            uiElements.Add(textbox);
            viewer.Children.Add(textbox);
            Inspector.SetRowAndColumn(textbox, 6, 6, 18, i);
            i += 6;
        }

        private void AddBoolCheckBox(Grid viewer, ref int i, FieldInfo field, bool val)
        {
            if (field.GetValue(component) is not bool curVal)
                return;

            var checkBox = Inspector.GetCheckBox(onCheckBoxChecked(field), i.ToString(), curVal);
            checkBox.Name = $"listBox{i}";
            checkBox.FontSize = 4;
            uiElements.Add(checkBox);
            viewer.Children.Add(checkBox);
            Inspector.SetRowAndColumn(checkBox, 1, 1, 22, i++ + 2);
            RoutedEventHandler onCheckBoxChecked(FieldInfo field)
            {
                return (e, o) =>
                {
                    o.Handled = true;
                    if (e is not CheckBox cb)
                        return;

                    if (cb.IsChecked is bool val)
                        field.SetValue(component, val);

                    UpdateData();
                };
            }
        }
        private void NewData()
        {
            if (component != null && data.Component.TryGetTarget(out var dataComp) && dataComp != component)
                data = new(component);
        }
        #region "Add" Button/TextBox/Grid methods
        private void AddTextBoxes(Grid viewer)
        {
            int i = 0;
            i = SerializeFields(viewer, i);
            i = SerializeMethods(viewer, i);
        }
        private void AddStringListTextBox(Grid viewer, ref int i, FieldInfo field, string[] strings, ref int strIndex, string str)
        {
            var label = Inspector.GetTextBox(i.ToString(), "default");
            var txtBox = Inspector.GetTextBox(str);
            txtBox.IsReadOnly = false;
            txtBox.Name = $"listBox{strIndex}";
            txtBox.KeyDown += (s, e) => TxtBox_KeyDown(s, e, field, component, strings);
            txtBox.LostKeyboardFocus += Input_LostKeyboardFocus;
            txtBox.GotKeyboardFocus += Input_GotKeyboardFocus;
            txtBox.FontSize = 4;
            label.FontSize = 4;

            uiElements.Add(txtBox);
            uiElements.Add(label);

            viewer.Children.Add(txtBox);
            viewer.Children.Add(label);
            Inspector.SetRowAndColumn(txtBox, lhs_height, lhs_width, 2, i);
            Inspector.SetRowAndColumn(label, rhs_height, rhs_width, 0, i++);
            strIndex++;
        }
        private int AddNestedComponentEditorButton(Grid viewer, ref int i, FieldInfo field, string name)
        {
            AddToEditor(viewer, i, "open editor for more options.", name, false);
            var button = Inspector.GetButton("Open Editor", new(0, 0, 0, 0));
            viewer.Children.Add(button);
            uiElements.Add(button);
            Inspector.SetControlColors(button, Brushes.Red, Brushes.Black);
            Inspector.SetRowAndColumn(button, 1, 3, 18, i++);
            button.FontSize = 3;
            button.Click += delegate
            {
                Component? Component = (Component)field.GetValue(component);
                if (Component is null)
                    pixel_editor.Console.Error("Cannot edit a null component.");
                Refresh(Component);
            };
            i++;
            return i;
        }
        private void AddToEditor(Grid viewer, int index, string? valStr, string name, bool bothColumns)
        {
            var nameDisplay = Inspector.GetTextBox(name);
            if (bothColumns)
            {
                var inputBox = Inspector.GetTextBox(valStr);
                Inspector.SetControlColors(inputBox, Brushes.DarkSlateGray, Brushes.White);
                inputBox.IsReadOnly = false;
                inputBox.GotKeyboardFocus += Input_GotKeyboardFocus;
                inputBox.LostKeyboardFocus += Input_LostKeyboardFocus;
                inputBox.KeyDown += InputBox_KeyDown;
                uiElements.Add(inputBox);
                viewer.Children.Add(inputBox);
                inputFields.Add(inputBox);
                Inspector.SetRowAndColumn(inputBox, 1, 4, 22, index + 1);
            }
            Inspector.SetControlColors(nameDisplay, Brushes.DarkSlateGray, Brushes.White);
            uiElements.Add(nameDisplay);
            viewer.Children.Add(nameDisplay);
            editEvents.Add((o, e) => SetVariable(o, e));
            Inspector.SetRowAndColumn(nameDisplay, 1, 4, 18, index + 1);
        }
        #endregion
        private bool SetVariable(string o, int i)
        {
            Inspector.GetComponentRuntimeInfo(component, out var fields, out _);

            foreach (var info in fields)
                if (info.Name == o)
                {
                    CommandParser.TryParse(inputFields[i].Text, out List<object> results);
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

            foreach (var element in uiElements)
            {
                var hasRefGlobal = mainGrid.Children.Contains(element);
                if (hasRefGlobal)
                    mainGrid.Children.Remove(element);

                var hasRefLocal = myGrid.Children.Contains(element);
                if (hasRefLocal)
                    myGrid.Children.Remove(element);
            }

            if (mainGrid != null)
            {
                if (mainGrid.Children.Contains(myGrid))
                    mainGrid.Children.Remove(myGrid);
            }

            uiElements?.Clear();
            myGrid?.Children.Clear();
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
        private void Update(object? sender, EventArgs e)
        {


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