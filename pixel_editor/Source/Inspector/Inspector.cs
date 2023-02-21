using System;
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
            Awake();
        }
        ComponentEditor? lastKnownComponentEditor; 

        public Node? selectedNode;

        #region
        private List<Control> activeControls = new();
        private List<Grid> activeGrids = new();
        
        private Grid MainGrid;
        private Grid grid;

        #endregion
        private Dictionary<Type, List<Component>> components = new();
        public static List<Action<Action>> EditActions = new();
        public static List<Action> EditActionArgs = new();
        
        public void Awake()
        {
            OnObjectSelected += Refresh;
            OnObjectDeselected += Refresh;
            OnComponentAdded += Refresh;
            OnComponentRemoved += Refresh;
            OnInspectorMoved += Inspector_OnInspectorMoved;
            RegisterAction((e) => DeselectNode(), System.Windows.Input.Key.Escape);

        }
        public void Update(object? sender, EventArgs e) { }

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
            OnComponentAdded?.Invoke(grid);
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
                OnComponentAdded?.Invoke(grid);
                return (true, t);
            }
            return (false, null);
        }

        #endregion
        #region UI Function
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

        bool newMenuOpen = false;
        Grid newMenuGrid;
        List<Action> AddItemActions = new();

        private void New(object sender, RoutedEventArgs e)
        {
            RefreshFunctions();
            e.Handled = true;
            if (!newMenuOpen)
            {
                newMenuOpen = true;
                newMenuGrid = Inspector.GetGrid();
                MainGrid.Children.Add(newMenuGrid);
                Inspector.SetRowAndColumn(newMenuGrid, 10, 10, Constants.InspectorPosition.x, Constants.InspectorPosition.y);

                int i = 0;
                foreach (var item in addComponentFunctions)
                {
                    Button button = Inspector.GetButton(item.Key, new(0, 0, 0, 0));
                    button.Name = $"button{i}";
                    AddItemActions.Add(() => AddObject(new(item.Key, item.Value)));
                    button.FontSize = 2;
                    button.Click += NewObjectButtonClicked;
                    newMenuGrid.Children.Add(button);
                    Inspector.SetRowAndColumn(button, 1, 2, 0, i);
                    i++;

                }
                return;
            }
            newMenuOpen = false;
            newMenuGrid.Children.Clear();
            newMenuGrid = null;
        }

        private void AddObject(KeyValuePair<string, object> item)
        {
            if (item.Value is Func<Component> funct)
            {
                Runtime.Log($"Component {nameof(funct.Method.ReturnType)} added!");
                funct.Invoke();
            }
        }

        private void NewObjectButtonClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RefreshFunctions();

            if (sender is not Button button) return;
            foreach (var item in addComponentFunctions)
                if (button.Name.ToInt() is int i && AddItemActions.Count > i)
                    AddItemActions[i]?.Invoke();
        }

        private void RefreshFunctions()
        {
            addComponentFunctions = new()
            {
                {"Player", selectedNode.AddComponent<pixel_renderer.Scripts.Player>},
                {"Animator", selectedNode.AddComponent<Animator>},
                {"Sprite", selectedNode.AddComponent<Sprite>},
                {"Collider", selectedNode.AddComponent<Collider>},
                {"Rigidbody", selectedNode.AddComponent<Rigidbody>},
            };
        }

       
        Dictionary<string, Func<Component>> addComponentFunctions; 

        private void AddComponentButton_Click(object sender, RoutedEventArgs e)
        {
            New(sender, e);
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
        private int AddComponentToInspector(Grid grid, int index, Component component)
        {
            var box = GetTextBox(component.Name);
            Button editComponentButton = GetEditComponentButton(index);
            grid.Children.Add(editComponentButton);
            grid.Children.Add(box);
            SetRowAndColumn(editComponentButton, 2, 2, 4, index * 2);
            SetRowAndColumn(box, 2, 4, 0, index * 2);
            EditActions.Add(component.OnEditActionClicked);
            EditActionArgs.Add(delegate
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
                if (button.Name.ToInt() is int i && EditActions.Count > i && EditActionArgs.Count > i)
                    EditActions[i]?.Invoke(EditActionArgs[i]);
                else Runtime.Log("Edit pressed failed.");
        }
        #endregion
    }
}