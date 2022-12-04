using System;
using System.Reflection;
using System.Collections.Generic;

using Brushes = System.Windows.Media.Brushes;

using System.Windows;
using System.Windows.Controls;

using pixel_renderer;

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
        private List<TextBlock> activeControls = new();
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
            Runtime.Instance.InspectorEventRaised += Instance_InspectorEventRaised;
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
            {
                foreach (var component in componentType)
                {
                    string info = GetComponentInfo(component);

                    TextBlock block = CreateBlock(info, thickness);

                    int rowSpan = info.Split('\n').Length * 2;

                    AddToInspector(index, block, rowSpan);

                    componentGrid.Children.Add(block);
                    componentGrid.UpdateLayout();

                    activeControls.Add(block);

                    index++;
                }
            }
            OnInspectorUpdated?.Invoke();
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

        public static void AddToInspector(int i, TextBlock component, int rowSpan)
        {
            component.SetValue(Grid.RowSpanProperty, rowSpan);
            component.SetValue(Grid.ColumnSpanProperty, 8);
            component.SetValue(Grid.RowProperty, i + i + i);
            component.SetValue(Grid.ColumnProperty, 6);
        }

        public static string GetComponentInfo(Component component)
        {
            IEnumerable<FieldInfo> fields = component.GetSerializedFields(); 
            string output = $"\b {component.Name} ";
            foreach (var field in fields)
            {
                var value = field.GetValue(component);
                output += $" \n \t{field} {value}";
            }
            return output;
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

        public static Label CreateLabel(string componentInfo, Thickness margin)
        {
            return new Label
            {
                Content = componentInfo,
                FontSize = 2.25f,
                Background = Brushes.DarkGray,
                Foreground = Brushes.White,
                Margin = margin,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic")
            };
        }
        public static TextBlock CreateBlock(string componentInfo, Thickness margin)
        {
            return new()
            {
                Text = componentInfo,
                FontSize = 2.25f,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic"),
                TextWrapping = TextWrapping.Wrap,
                Background = Brushes.DarkGray,
                Foreground = Brushes.White,
                Margin = margin,
                Height = double.NaN,
                Width = double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        public static Button CreateButton(string content, Thickness margin) => new Button()
        {
            Content = content,
            Margin = margin,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1, 1, 1, 1),
        };
        
        private void Instance_InspectorEventRaised(InspectorEvent e)
        {
            if (Runtime.Instance.IsRunning) 
                    Runtime.Instance.Toggle();  

            var msg = MessageBox.Show(e.expression.ToString(),
                                      e.message, 
                                      MessageBoxButton.YesNo);

            var args = e.expressionArgs;

            if (args.Length < 4) return;
                 e.expression(args[0], args[1] ?? null, args[2] ?? null, args[3] ?? null);
        }
    }
}