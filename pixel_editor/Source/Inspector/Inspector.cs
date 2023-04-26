﻿using System;
using System.Reflection;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using static pixel_core.Input;
using System.Windows;
using System.Windows.Controls;
using pixel_core;
using System.Linq;
using System.Windows.Media;
using System.Drawing.Text;
using pixel_core.types.Components;

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
            RegisterAction(this, DeselectNode, System.Windows.Input.Key.Escape);
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
                $"{component.node.Name} \n {component.Name} \n"
            };

            foreach (var field in fields)
            {
                string valString = "";
                var value = field.GetValue(component);
                valString = value.ToString();
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
        public static void GetComponentInfo(Editor component, out IEnumerable<FieldInfo> fields, out IEnumerable<PropertyInfo> properties)
        {
            fields = component.GetType().GetFields();
            properties = component.GetType().GetProperties();
        }
        #endregion
        #region Node Function
        public Node? lastSelectedNode;
        public void DeselectNode()
        {
            if (lastSelectedNode != null)
            {
                lastSelectedNode = null;
                
                foreach( var x in activeGrids)
                    x.Visibility = Visibility.Collapsed;
               
                activeControls.Clear();
                activeGrids.Clear();

                Editor.Current.componentEditor?.Dispose();

                OnObjectDeselected?.Invoke(grid);
                if (addComponentMenuOpen)
                    AddComponentButton_Click(new(), new());
            }
            Runtime.Current.stagingHost.DeselectNode();
            lastSelectedNode = null;
        }
        public void SelectNode(Node node)
        {
            lastSelectedNode = node;
            if(node != null)
            foreach (var comp in node.Components)
                foreach(var component in comp.Value) component.selected_by_editor = true;

            OnObjectSelected?.Invoke(grid);
        }

        #endregion
        #region UI Function
        
        private List<Control> activeControls = new();
        private List<Grid> activeGrids = new();
        bool addComponentMenuOpen = false;

        private Grid MainGrid;
        private Grid grid;
        Grid addComponentGrid;

        Dictionary<string, Func<Component>> addComponentFunctions;
        public static List<Action> editComponentActions = new();
        private Dictionary<Type, List<Component>> components = new();
        List<Action> addComponentActions = new();
        
        private static void RemoveComponent(Component obj)
        {
            obj.node.RemoveComponent(obj);
        }
        private void Refresh(Grid grid)
        {
            if (grid != null)
            {
                grid.Children.Clear();
                grid.Visibility = Visibility.Hidden;
            }

            activeGrids.Clear();
            activeControls.Clear();
            editComponentActions.Clear();


            grid = null;
            grid = NewInspectorGrid();

            if (lastSelectedNode == null)
                return;

            components = lastSelectedNode.Components;

            int index = 0;

            index = AddTransform(grid, index);

            foreach (var componentType in components.Values)
                foreach (var component in componentType)
                    index = AddComponentToInspector(grid, index, component);


            var addComponentButton = Inspector.GetButton("Add Component", new(0, 0, 0, 0));
            addComponentButton.FontSize = 2;
            addComponentButton.Click += AddComponentButton_Click;
            SetRowAndColumn(addComponentButton, 2, 3, 0, index * 2 + 1);
            grid.Children.Add(addComponentButton);


            OnInspectorUpdated?.Invoke(grid);
        }

        private int AddTransform(Grid grid, int index)
        {
            pixel_core.Node.TransformComponent transform = new()
            {
                node = lastSelectedNode,
                position = lastSelectedNode.Position,
                scale = lastSelectedNode.Scale,
                rotation = lastSelectedNode.Rotation
            };


            index = AddComponentToInspector(grid, index, transform, false, lastSelectedNode.Name);
            return index;
        }

        private int AddComponentToInspector(Grid grid, int index, Component component, bool removable = true, string? name = null)
        {
            var box = GetTextBox(name ?? component.GetType().Name);
            SetRowAndColumn(box, 2, 4, 0, index * 2);
            grid.Children.Add(box);

            Button editComponentButton = GetEditComponentButton(index);
            SetRowAndColumn(editComponentButton, 2, 2, 4, index * 2);
            grid.Children.Add(editComponentButton);
           
            if (removable)
            {
                Button removeButton = GetRemoveComponentButton(index, component);
                SetRowAndColumn(removeButton, 2, 2, 6, index * 2);
                grid.Children.Add(removeButton);
            }


            editComponentActions.Add(delegate
            {
                Editor.Current.componentEditor ??= new();
                Editor.Current.componentEditor.Dispose();
                Editor.Current.componentEditor.Refresh(component);
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
                //TODO: Make it so the component list/ data refreshes on click here.
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
            ClearAddComponentTray();
        }
        private void RefreshAddComponentFunctions()
        {
            addComponentFunctions = new()
            {
                {"Sprite",    AddSprite},
                {"Collider",  AddCollider},
                {"Animator",  AddAnimator},
                {"Rigidbody", AddRigidbody},
                {"Softbody",  AddSoftbody},
                {"Lua",       AddLuaComponent},
                {"Particles", AddParticles},
                {"Player",    AddPlayer},
                {"Joint",     AddJoint},
            };
        }
        private void AddComponentButton_Click(object sender, RoutedEventArgs e)
        {
            if(e.RoutedEvent != null)
                e.Handled = true;

            RefreshAddComponentFunctions();
            if (!addComponentMenuOpen)
            {
                PopulateAddComponentTray();
                return;
            }
            ClearAddComponentTray();
        }
        private void PopulateAddComponentTray()
        {
            addComponentMenuOpen = true;
            addComponentGrid = GetGrid();
            MainGrid.Children.Add(addComponentGrid);
            SetRowAndColumn(addComponentGrid, 10, 10, (int)Editor.Current.settings.InspectorPosition.X, (int)Editor.Current.settings.InspectorPosition.Y);

            int i = 0;
            foreach (var item in addComponentFunctions)
            {
                Button button = GetButton(item.Key, new(0, 0, 0, 0));
                button.Name = $"button{i}";
                addComponentActions.Add(() => AddComponent(new(item.Key, item.Value)));
                button.FontSize = 2;
                button.Click += AddComponentClicked;
                addComponentGrid.Children.Add(button);
                SetRowAndColumn(button, 1, 2, 0, i);
                i++;

            }
            return;
        }
        private void ClearAddComponentTray()
        {
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
            SetRowAndColumn(grid, Editor.Current.settings.InspectorWidth , Editor.Current.settings.InspectorHeight, (int)Editor.Current.settings.InspectorPosition.X, (int)Editor.Current.settings.InspectorPosition.Y);
        }
        public static TextBox GetTextBox(string componentName, string style = "default")
        {
            return style switch
            {
                "default" => DefaultStyle(componentName),
                "mint" => MintStyle(componentName),
                _ => DefaultStyle(componentName),
            };
        }
        public static System.Drawing.FontFamily[] GetFonts()
        {
            InstalledFontCollection installedFontCollection = new();
            System.Drawing.FontFamily[] fonts = installedFontCollection.Families;
            return fonts;
        }

        private static TextBox MintStyle(string componentName)
        {
            return new()
            {
                Text = componentName,
                FontSize = 4f,
                FontFamily = new FontFamily("MS Gothic"),
                IsReadOnly = true,

                AcceptsReturn = false,
                AcceptsTab = false,
                AllowDrop = false,

                TextWrapping = TextWrapping.NoWrap,

                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,

                BorderThickness = new Thickness(0.1, 0.1, 0.1, 0.1),
                BorderBrush = Brushes.Black,
                Foreground = Brushes.Black,
                Background = Brushes.Teal,

            };
        }
        private static TextBox DefaultStyle(string componentName)
        {
            return new()
            {
                Text = componentName,
                FontSize = 4f,
                FontFamily = new FontFamily("MS Gothic"),
                IsReadOnly = true,

                AcceptsReturn = false,
                AcceptsTab = false,
                AllowDrop = false,

                TextWrapping = TextWrapping.NoWrap,

                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,

                BorderThickness = new Thickness(0.1, 0.1, 0.1, 0.1),
                Foreground = Brushes.White,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,30,30,30)),

            };
        }
        public static Button GetButton(string content, Thickness margin) => new Button()
        {
            Content = content,
            Margin = margin,
            BorderThickness = new Thickness(0.1,0.1,0.1,0.1),
        };
        private static Button GetRemoveComponentButton(int index, Component component)
        {
            Button editComponentButton = GetButton("Remove", new(0, 0, 0, 0));
            editComponentButton.FontSize = 3;
            editComponentButton.Click += (_, e) =>
            {
                e.Handled = true; 
                RemoveComponent(component);
            };
            editComponentButton.Name = "remove_button_" + index.ToString();
            return editComponentButton;
        }
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
            Editor.Current.settings.InspectorPosition.X = x;
            Editor.Current.settings.InspectorPosition.Y = y;
        }
        private static void HandleEditPressed(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                if (button.Name.ToInt() is int i && editComponentActions.Count > i && editComponentActions.Count > i)
                    editComponentActions[i]?.Invoke();
                else Runtime.Log("Edit pressed failed.");
        }
        internal static void GetComponentRuntimeInfo(Component component, out IEnumerable<FieldInfo> fields, out IEnumerable<PropertyInfo> properties, out IEnumerable<MethodInfo> methods)
        {
            fields = component.GetType().GetRuntimeFields();
            properties = component.GetType().GetRuntimeProperties();
            methods = component.GetType().GetRuntimeMethods();
        }
        public static IEnumerable<FieldInfo> GetSerializedFields(Component component) =>
           from FieldInfo field in component.GetType().GetRuntimeFields()
           from CustomAttributeData data in field.CustomAttributes
           where data.AttributeType == typeof(FieldAttribute)
           select field;
        public static IEnumerable<MethodInfo> GetSerializedMethods(Component component) =>
           from MethodInfo method in component.GetType().GetRuntimeMethods()
           from CustomAttributeData data in method.CustomAttributes
           where data.AttributeType == typeof(MethodAttribute)
           select method;

        //public static IEnumerable<PropertyInfo> GetSerializedProperties(this Component component) =>
        //   from PropertyInfo field in component.GetType().GetRuntimeProperties()
        //   from CustomAttributeData data in field.CustomAttributes
        //   where data.AttributeType == typeof(null)
        //   select field;

        Lua AddLuaComponent()
        {
            return lastSelectedNode?.AddComponent<Lua>();
        }
        Joint AddJoint()
        {
            return lastSelectedNode?.AddComponent<Joint>();  
        }
        ParticleSystem AddParticles()
        {
            return lastSelectedNode?.AddComponent<ParticleSystem>(); 
        }
        Sprite AddSprite()
        {
            var x = lastSelectedNode?.AddComponent<Sprite>();
            Runtime.Log($"Sprite Added!");
            return x;
        }
        Player AddPlayer()
        {
            var x = lastSelectedNode?.AddComponent<Player>();
            Runtime.Log($"Player Added!");
            return x;
        }
        Animator AddAnimator()
        {
            var x = lastSelectedNode?.AddComponent<Animator>();
            Runtime.Log($"Animator Added!");

            return x;
        }
        Collider AddCollider()
        {
            var x = lastSelectedNode.AddComponent<Collider>();
            Runtime.Log($"Collider Added!");
            return x;
        }
        Softbody AddSoftbody()
        {
            var x = lastSelectedNode.AddComponent<Softbody>();
            Runtime.Log($"Player Added!");
            return x;
        }
        Rigidbody AddRigidbody()
        {
            var x = lastSelectedNode.AddComponent<Rigidbody>();
            Runtime.Log($"Rigidbody Added!");
            return x;
        }

        public static CheckBox GetCheckBox(RoutedEventHandler e, string content = "", bool currentValue = false)
        {
            CheckBox box = new()
            {
                Content = content, 
            };
            box.Click += e;
            return box; 
        }

        internal static TextBox GetInputField(string curVal)
        {
            TextBox box = new()
            {
                Text = curVal,
            };
            return box;
        }

        #endregion
    }
}