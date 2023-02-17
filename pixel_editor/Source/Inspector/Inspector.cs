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
            this.name = name ?? new();

            this.objInfo = objInfo ?? new();
            this.componentGrid = componentGrid ?? new();
            
            this.name.Content = "_";
            this.objInfo.Content = "_";
            
            Awake();
        }

        public Node? loadedNode;
        private List<Control> activeControls = new();
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
                    TextBox block = CreateBlock(info);

                    int lineCt = info.Split('\n').Length ;

                    block.Width = 250;
                    block.Height = Math.Max(3 * lineCt, 5);

                    SetRowAndColumn(block, 1, 4, 0, index + 2);
                    AddControlToInspector(block);

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

        public void DeselectNode()
        {
            if (loadedNode != null)
            {
                loadedNode = null;
                foreach (var control in activeControls)
                {
                    componentGrid.Children.Remove(control);
                }
                activeControls.Clear();
                OnObjectDeselected?.Invoke();
            }
        }
        public void SelectNode(Node node)
        {
            loadedNode = node;
            OnObjectSelected?.Invoke();
        }
        public static void SetRowAndColumn(Control component, int height, int width, int x, int y)
        {
            component.SetValue(Grid.RowSpanProperty, height);
            component.SetValue(Grid.ColumnSpanProperty, width);
            component.SetValue(Grid.RowProperty, y);
            component.SetValue(Grid.ColumnProperty, x);
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
        public static Label CreateLabel(string componentInfo)
        {
            return new Label
            {
                Content = componentInfo,
                FontSize = 3f,
                Foreground = Brushes.White,
                Padding = new(5, 5, 5, 5),
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic")
            };
        }
        public static TextBox CreateBlock(string componentInfo)
        {
            return new()
            {
                Text = componentInfo,
                
                FontSize = 3f,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic"),
                
                AcceptsReturn = true, AcceptsTab = true, AllowDrop = true,
                TextWrapping = TextWrapping.Wrap,
                
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Left, 
                VerticalAlignment= VerticalAlignment.Top,
                Foreground = Brushes.White,
                Background = Brushes.Black,
                
            };
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