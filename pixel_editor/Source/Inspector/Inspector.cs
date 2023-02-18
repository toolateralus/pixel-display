using System;
using System.Reflection;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using System.Windows;
using System.Windows.Controls;
using pixel_renderer;
using System.Windows.Media.Effects;
using System.Linq;

namespace pixel_editor
{
    public class Inspector
    {
        public Inspector(Label name, Label objInfo, Grid componentGrid)
        {
            this.name = name;

            this.objInfo = objInfo;
            this.componentGrid = componentGrid;
            
            this.name.Content = "_";
            this.objInfo.Content = "_";
            
            Awake();
        }

        public Node? selectedNode;
        private List<Control> activeControls = new();
        private List<Grid> activeGrids = new();
        
        private Label name;
        private Label objInfo;

        private Grid componentGrid; 
        
        private Dictionary<Type, List<Component>> components = new();

        private Grid inspectorGrid;

        public static List<Action<Action>> EditActions = new();
        public static List<Action> EditActionArgs = new(); 

        public event Action<Grid> OnObjectSelected;
        public event Action<Grid> OnObjectDeselected;
        public event Action<Grid> OnInspectorUpdated;
        public event Action<Grid> OnComponentAdded;
        public event Action<Grid> OnComponentRemoved;

        public (bool, T) TryGetComponent<T>() where T : Component, new()
        {
            if (selectedNode is null)
                return (false, null);

            var hasComp = selectedNode.TryGetComponent(out T obj);
            return (hasComp, obj); 

        }
        public (bool, T) TryAddComponent<T>() where T : Component, new()
        {
            if (selectedNode is null)
                return (false, null);

            T t = new();
            var obj = selectedNode.AddComponent(t);
            OnComponentAdded?.Invoke(inspectorGrid);
            return (true, obj);

        }
        public (bool, T) TryRemoveComponent<T>() where T : Component, new()
        {
            if (selectedNode is null)
                return (false, null);
            T t = new();
            if (selectedNode is not null && selectedNode.HasComponent<T>()) 
            {
                selectedNode.RemoveComponent(t);
                OnComponentAdded?.Invoke(inspectorGrid);
                return (true, t);
            }
            return (false, null);
        } 

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


                // default value type print
                else valString = value?.ToString();
                output.Add($" \n{field.Name} {valString}");
            }
            string output_string = string.Empty;
            foreach (var str in output)
            {
                if(str != string.Empty || !string.IsNullOrEmpty(str))
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

        public void Awake()
        {
            OnObjectSelected += Refresh;
            OnObjectDeselected += Refresh;
            OnComponentAdded += Refresh;
            OnComponentRemoved += Refresh;
        }
        public void Update(object? sender, EventArgs e) { }
        ComponentEditor? lastKnownComponentEditor; 
        private void Refresh(Grid grid)
        {
            if (selectedNode == null) return;
            name.Content = selectedNode.Name;
            components = selectedNode.Components;
            objInfo.Content = $"#{components.Count} Components";
            var thickness = new Thickness(0, 0, 0, 0);
            int index = 0;

            grid = GetNodeInspectorGrid();
            
            AddGridToInspector(grid);

            SetRowAndColumn(grid, 12, 8, 0, index);

            foreach (var componentType in components.Values)
                foreach (var component in componentType)
                {

                    var box = GetTextBox(component.Name);
                    
                    Button button = CreateButton("Edit", new(0, 0, 0, 0));
                    
                    button.FontSize = 4;

                    button.Click += HandleEditPressed;

                    EditActions.Add(component.OnEditActionClicked);
                    EditActionArgs.Add(delegate 
                    {
                      lastKnownComponentEditor = new ComponentEditor(Editor.Current, component);
                        lastKnownComponentEditor.Show();
                    });

                    button.Name = "edit_button_" + index.ToString();

                    grid.Children.Add(button);
                    grid.Children.Add(box);

                    SetRowAndColumn(button, 2, 2, 4, index * 2);
                    SetRowAndColumn(box, 2, 4, 0, index * 2);

                    index++;
                }

            OnInspectorUpdated?.Invoke(inspectorGrid);
        }

        public void DeselectNode()
        {
            if (selectedNode != null)
            {
                selectedNode = null;

                foreach (var control in activeControls) componentGrid.Children.Remove(control);

                activeControls.Clear();

                OnObjectDeselected?.Invoke(inspectorGrid);
            }
        }
        public void SelectNode(Node node)
        {
            selectedNode = node;
            OnObjectSelected?.Invoke(inspectorGrid = GetNodeInspectorGrid());
        }

        public static void SetRowAndColumn(Grid grid, int height, int width, int x, int y)
        {
            grid.SetValue(Grid.RowSpanProperty, height);
            grid.SetValue(Grid.ColumnSpanProperty, width);
            grid.SetValue(Grid.RowProperty, y);
            grid.SetValue(Grid.ColumnProperty, x);
        }
        private static TextBox GetTextBox(string componentName)
        {
            TextBox box = new()
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


                    Foreground = Brushes.Green,
                    Background = Brushes.Black,

                };
            return box;
        }



        private void AddGridToInspector(Grid control)
        {
            componentGrid.Children.Add(control);
            componentGrid.UpdateLayout();
            activeGrids.Add(control);
        }

        public static void SetRowAndColumn(Control control, int height, int width, int x, int y)
        {
            control.SetValue(Grid.RowSpanProperty, height);
            control.SetValue(Grid.ColumnSpanProperty, width);
            control.SetValue(Grid.RowProperty, y);
            control.SetValue(Grid.ColumnProperty, x);
        }
        private static Grid GetNodeInspectorGrid(int width = 18, int height = 24, int colWidth = 12, int rowHeight = 12)
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
        public static Button CreateButton(string content, Thickness margin) => new Button()
        {
            Content = content,
            Margin = margin,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1, 1, 1, 1),
        };

        #region Events
        private static void HandleEditPressed(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
                if (button.Name.ToInt() is int i && EditActions.Count > i && EditActionArgs.Count > i)
                    EditActions[i]?.Invoke(EditActionArgs[i]);
                else Runtime.Log("Edit pressed failed.");
        }
        #endregion

    }
}