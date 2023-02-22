﻿using System;
using System.Reflection;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using static pixel_renderer.Input;
using System.Windows;
using System.Windows.Controls;
using pixel_renderer;
using System.Windows.Media.Effects;
using System.Linq;
using System.Windows.Media;
using System.Net.NetworkInformation;

namespace pixel_editor
{
    public class Inspector
    {
        public Inspector(Grid grid)
        {
            MainGrid = grid;
            OnObjectSelected += Refresh;
            OnObjectDeselected += Refresh;
            OnComponentAdded += Refresh;
            OnComponentRemoved += Refresh;
            OnInspectorMoved += Inspector_OnInspectorMoved;
            RegisterAction((e) => DeselectNode(), System.Windows.Input.Key.Escape);
        }
        public void Update(object? sender, EventArgs e)
        {


        }
        #region Reflection Functions
        public static string GetComponentInfo(Component component)
        {
            IEnumerable<FieldInfo> fields = component.GetSerializedFields();

            List<string> output = new()
            {
                $"{component.parent.Name} \n {component.Name} \n"
            };

            foreach (var field in fields)
            {
                string valString = "";
                var value = field.GetValue(component);

                if (field.FieldType == typeof(Vec2))
                    valString = ((Vec2)value).AsString();


                else valString = value?.ToString();
                output.Add($" \n{field.Name} {valString}");
            }
            string output_string = string.Empty;
            foreach (var str in output)
            {
                if (str != string.Empty || !string.IsNullOrEmpty(str))
                    output_string += str;
                Runtime.Log(output_string);
            }
            return output_string;
        }
        public static void GetComponentRuntimeInfo(Component component, out IEnumerable<FieldInfo> fields, out IEnumerable<PropertyInfo> properties)
        {
            fields = component.GetType().GetRuntimeFields();
            properties = component.GetType().GetRuntimeProperties();

        }
        public static void GetComponentInfo(Component component, out IEnumerable<FieldInfo> fields, out IEnumerable<PropertyInfo> properties)
        {
            fields = component.GetType().GetFields();
            properties = component.GetType().GetProperties();
        }
        #endregion
        #region Node Function
        public Node? selectedNode;
        public void DeselectNode()
        {
            if (selectedNode != null)
            {
                selectedNode = null;
                
                foreach( var x in activeGrids)
                    x.Visibility = Visibility.Collapsed;
               
                activeControls.Clear();
                activeGrids.Clear();

                OnObjectDeselected?.Invoke(grid);
            }
            Runtime.Current.stagingHost.DeselectNode();
        }
        public void SelectNode(Node node)
        {
            selectedNode = node;
            OnObjectSelected?.Invoke(grid);
        }

        #endregion
        #region UI Function
        ComponentEditor? lastKnownComponentEditor;

        List<Action> addComponentActions = new();
        Dictionary<string, Func<Component>> addComponentFunctions;
        Grid addComponentGrid;
        
        bool addComponentMenuOpen = false;
        
        private List<Control> activeControls = new();
        private List<Grid> activeGrids = new();

        private Grid MainGrid;
        private Grid grid;

        private Dictionary<Type, List<Component>> components = new();
        
        public static List<Action<Action>> editComponentActions = new();
        public static List<Action> editComponentActionArgs = new();
        
        private void Refresh(Grid grid)
        {
            activeGrids.Clear();
            activeControls.Clear();

            grid = null;

            grid = NewInspectorGrid();

            if (selectedNode == null)
                return;

            components = selectedNode.Components;

            int index = 0;

            foreach (var componentType in components.Values)
                foreach (var component in componentType)
                    index = AddComponentToInspector(grid, index, component);

            var addComponentButton = Inspector.GetButton("Add Component", new(0, 0, 0, 0));
            addComponentButton.FontSize = 2;
            grid.Children.Add(addComponentButton);
            addComponentButton.Click += AddComponentButton_Click;
            Inspector.SetRowAndColumn(addComponentButton, 2, 3, 0, index * 2 + 1);

            OnInspectorUpdated?.Invoke(grid);
        }
     
        private int AddComponentToInspector(Grid grid, int index, Component component)
        {
            var box = GetTextBox(component.Name);
            Button editComponentButton = GetEditComponentButton(index);
            grid.Children.Add(editComponentButton);
            grid.Children.Add(box);
            SetRowAndColumn(editComponentButton, 2, 2, 4, index * 2);
            SetRowAndColumn(box, 2, 4, 0, index * 2);
            editComponentActions.Add(component.OnEditActionClicked);
            editComponentActionArgs.Add(delegate
            {
                lastKnownComponentEditor = new ComponentEditor(Editor.Current, component);
                lastKnownComponentEditor.Show();
            });
            index++;
            return index;
        }
        public void AddGridToInspector(Grid grid)
        {
            this.MainGrid.Children.Add(grid);
            this.MainGrid.UpdateLayout();
            activeGrids.Add(grid);
        }

        private void AddComponent(KeyValuePair<string, object> item)
        {
            if (item.Value is Func<Component> funct)
            {
                Runtime.Log($"Component {nameof(funct.Method.ReturnType)} added!");
                funct.Invoke();
            }
        }
        private void AddComponentClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RefreshAddComponentFunctions();

            if (sender is not Button button) return;
            foreach (var item in addComponentFunctions)
                if (button.Name.ToInt() is int i && addComponentActions.Count > i)
                {
                    addComponentActions[i]?.Invoke();
                    return; 
                }
        }

        private void RefreshAddComponentFunctions()
        {
            addComponentFunctions = new()
            {
                {"Player",    AddPlayer},
                {"Animator",  AddAnimator},
                {"Sprite",    AddSprite},
                {"Collider",  AddCollider},
                {"Rigidbody", AddRigidbody},
            };

            pixel_renderer.Scripts.Player AddPlayer()
            {
                var x= selectedNode.AddComponent<pixel_renderer.Scripts.Player>();
                Runtime.Log($"Player Added!");
                return x;
            }

            Animator AddAnimator()
            {
                var x= selectedNode.AddComponent<Animator>();
                Runtime.Log($"Animator Added!");

                return x;
            }

            Sprite AddSprite()
            {
                var x = selectedNode.AddComponent<Sprite>();
                Runtime.Log($"Sprite Added!");
                return x;
            }

            Collider AddCollider()
            {
                var x = selectedNode.AddComponent<Collider>();
                Runtime.Log($"Collider Added!");
                return x;
            }

            Rigidbody AddRigidbody()
            {
                var x= selectedNode.AddComponent<Rigidbody>();
                Runtime.Log($"Rigidbody Added!");
                return x;
            }
        }
        private void AddComponentButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RefreshAddComponentFunctions();
            if (!addComponentMenuOpen)
            {
                addComponentMenuOpen = true;
                addComponentGrid = Inspector.GetGrid();
                MainGrid.Children.Add(addComponentGrid);
                Inspector.SetRowAndColumn(addComponentGrid, 10, 10, Constants.InspectorPosition.x, Constants.InspectorPosition.y);

                int i = 0;
                foreach (var item in addComponentFunctions)
                {
                    Button button = Inspector.GetButton(item.Key, new(0, 0, 0, 0));
                    button.Name = $"button{i}";
                    addComponentActions.Add(() => AddComponent(new(item.Key, item.Value)));
                    button.FontSize = 2;
                    button.Click += AddComponentClicked;
                    addComponentGrid.Children.Add(button);
                    Inspector.SetRowAndColumn(button, 1, 2, 0, i);
                    i++;

                }
                return;
            }
            addComponentMenuOpen = false;
            addComponentGrid.Children.Clear();
            addComponentGrid = null;
        }
        
        private Grid NewInspectorGrid()
        {
            Grid grid = GetGrid();
            AddGridToInspector(grid);
            RePositionInspectorGrid(grid);
            return grid;
        }
        private static void RePositionInspectorGrid(Grid grid)
        {
            SetRowAndColumn(grid, Constants.InspectorWidth , Constants.InspectorHeight, Constants.InspectorPosition.x, Constants.InspectorPosition.y);
        }
        public static TextBox GetTextBox(string componentName, string style = "default")
        {
            return style switch
            {
                _ => DefaultStyle(componentName),
            };
        }
        private static TextBox DefaultStyle(string componentName)
        {
            return new()
            {
                Text = componentName,
                FontSize = 4f,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic"),
                IsReadOnly = true,

                AcceptsReturn = false,
                AcceptsTab = false,
                AllowDrop = false,

                TextWrapping = TextWrapping.NoWrap,

                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,

                BorderThickness = new Thickness(0.1, 0.1, 0.1, 0.1),
                Foreground = Brushes.Black,
                Background = Brushes.Gray,

            };
        }
        public static Button GetButton(string content, Thickness margin) => new Button()
        {
            Content = content,
            Margin = margin,
            BorderThickness = new Thickness(0.1,0.1,0.1,0.1),
        };
        private static Button GetEditComponentButton(int index)
        {
            Button editComponentButton = GetButton("Edit", new(0, 0, 0, 0));
            editComponentButton.FontSize = 3;
            editComponentButton.Click += HandleEditPressed;
            editComponentButton.Name = "edit_button_" + index.ToString();
            return editComponentButton;
        }
        
        public static Grid GetGrid(int width = 18, int height = 24, int colWidth = 12, int rowHeight = 12)
        {
            Grid grid = new()
            {
                ClipToBounds = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var rows = grid.RowDefinitions;
            var cols = grid.ColumnDefinitions;

            for (int x = 0; x < width; ++x)
            {
                ColumnDefinition col = new();
                col.Width = new GridLength(colWidth);
                cols.Add(col);
            }

            for (int x = 0; x < height; ++x)
            {
                RowDefinition row = new();
                row.Height = new GridLength(rowHeight);
                rows.Add(new());
            }

            return grid;
        }
        public static void SetRowAndColumn(Grid grid, int height, int width, int x, int y)
        {
            grid.SetValue(Grid.RowSpanProperty, height);
            grid.SetValue(Grid.ColumnSpanProperty, width);
            grid.SetValue(Grid.RowProperty, y);
            grid.SetValue(Grid.ColumnProperty, x);
        }
        public static void SetRowAndColumn(Control control, int height, int width, int x, int y)
        {
            control.SetValue(Grid.RowSpanProperty, height);
            control.SetValue(Grid.ColumnSpanProperty, width);
            control.SetValue(Grid.RowProperty, y);
            control.SetValue(Grid.ColumnProperty, x);
        }
        public static void SetControlColors(Control control, SolidColorBrush background, SolidColorBrush foreground)
        {
            control.Foreground = foreground;
            control.Background = background;
        }

        #endregion
        #region Events
        public static event Action<Grid> OnObjectSelected;
        public static event Action<Grid> OnObjectDeselected;
        public static event Action<Grid> OnInspectorUpdated;
        public static event Action<Grid> OnComponentAdded;
        public static event Action<Grid> OnComponentRemoved;
        public static Action<int, int> OnInspectorMoved;


        private void Inspector_OnInspectorMoved(int x = 0, int y = 0)
        {
            Constants.InspectorPosition.x = x;
            Constants.InspectorPosition.y = y;
            Refresh(grid);
        }
        private static void HandleEditPressed(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                if (button.Name.ToInt() is int i && editComponentActions.Count > i && editComponentActionArgs.Count > i)
                    editComponentActions[i]?.Invoke(editComponentActionArgs[i]);
                else Runtime.Log("Edit pressed failed.");
        }
        #endregion
    }
}