using System;
using System.Reflection;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using System.Windows;
using System.Windows.Controls;
using pixel_renderer;
using System.Windows.Media.Effects;

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

        public Node? loadedNode;
        private List<Control> activeControls = new();
        private List<Grid> activeGrids = new();
        private Label name;
        private Label objInfo;
        private Grid componentGrid; 
        
        private Dictionary<Type, List<Component>> components = new();

        public event Action OnObjectSelected;
        public event Action OnObjectDeselected;
        
        public event Action OnInspectorUpdated;

        public event Action OnComponentAdded;
        public event Action OnComponentRemoved;

        public void Awake()
        {
            OnObjectSelected += Refresh;
            OnObjectDeselected += Refresh;
            OnComponentAdded += Refresh;
            OnComponentRemoved += Refresh;
        }
        public void Update(object? sender, EventArgs e) { }
        
        private void Refresh()
        {
            if (loadedNode == null) return;
            name.Content = loadedNode.Name;
            components = loadedNode.Components;
            objInfo.Content = $"#{components.Count} Components";
            var thickness = new Thickness(0, 0, 0, 0);
            int index = 0;

            foreach (var componentType in components.Values)
                foreach (var component in componentType)
                {
                    string info = GetComponentInfo(component);
                    Grid block = CreateGrid(info);

                    int lineCt = info.Split('\n').Length ;

                    block.Width = 150;
                    //block.Height = Math.Max(5 * lineCt, 25);

                    SetRowAndColumn(block, 4, 8, 0, index);
                    AddGridToInspector(block);

                    index++;
                }

            OnInspectorUpdated?.Invoke();
        }

        private void AddControlToInspector(Control control)
        {
            componentGrid.Children.Add(control);
            componentGrid.UpdateLayout();
            activeControls.Add(control);
        }
        private void AddGridToInspector(Grid control)
        {
            componentGrid.Children.Add(control);
            componentGrid.UpdateLayout();
            activeGrids.Add(control);
        }

        public void DeselectNode()
        {
            if (loadedNode != null)
            {
                loadedNode = null;

                foreach (var control in activeControls) componentGrid.Children.Remove(control);

                activeControls.Clear();
                OnObjectDeselected?.Invoke();
            }
        }
        public void SelectNode(Node node)
        {
            loadedNode = node;
            OnObjectSelected?.Invoke();
        }
        public static void SetRowAndColumn(Control control, int height, int width, int x, int y)
        {
            control.SetValue(Grid.RowSpanProperty, height);
            control.SetValue(Grid.ColumnSpanProperty, width);
            control.SetValue(Grid.RowProperty, y);
            control.SetValue(Grid.ColumnProperty, x);
        }
        public static void SetRowAndColumn(Grid grid, int height, int width, int x, int y)
        {
            grid.SetValue(Grid.RowSpanProperty, height);
            grid.SetValue(Grid.ColumnSpanProperty, width);
            grid.SetValue(Grid.RowProperty, y);
            grid.SetValue(Grid.ColumnProperty, x);
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
     
        public static Grid CreateGrid(string componentInfo)
        {
            Grid grid = new()
            {
                ClipToBounds = true,
                Height = 100,
                Width = 500,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top, 
            };
            var rows = grid.RowDefinitions;
            var cols = grid.ColumnDefinitions;

            for (int x = 0; x < 12; ++x)
            {
                ColumnDefinition col = new();

                col.Width = new GridLength(48);
                cols.Add(col);
                

                if (x % 2 == 0)
                {
                    RowDefinition row = new();
                    row.Height = new GridLength(36);
                    rows.Add(new());    
                }

            }

            int i = 0;

            foreach (var item in componentInfo.Split('\n'))
            {
                TextBox box = new()
                {
                    Text = item,

                    FontSize = 4f,
                    FontFamily = new System.Windows.Media.FontFamily("MS Gothic"),

                    AcceptsReturn = false,
                    AcceptsTab = false,
                    AllowDrop = false,
                    TextWrapping = TextWrapping.Wrap,

                    HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,

                    Foreground = Brushes.White,
                    Background = Brushes.Black,

                };
                grid.Children.Add(box);
                i++;
                SetRowAndColumn(box, 1, 4, 0, i * 2);
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


    }
}