using System;
using System.Reflection;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using static Pixel.Input;
using System.Windows;
using System.Windows.Controls;
using Pixel;
using System.Linq;
using System.Windows.Media;
using System.Drawing.Text;
using Pixel.Types.Components;

namespace Pixel_Editor
{
    public class Inspector
    {
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
        public Inspector()
        {
            itemsControl = Editor.Current.inspectorControl;
            OnObjectSelected += RefreshInspector;
            OnObjectDeselected += RefreshInspector;
            OnComponentAdded += RefreshInspector;
            OnComponentRemoved += RefreshInspector;
            OnInspectorMoved += Inspector_OnInspectorMoved;
            RegisterAction(this, DeselectNode, System.Windows.Input.Key.Escape);
        }
        
        #region Node Function
        public Node? lastSelectedNode;
        public void DeselectNode()
        {
            if (lastSelectedNode != null)
            {
                lastSelectedNode = null;

                activeControls.Clear();

                Editor.Current.componentEditor?.Dispose();

                OnObjectDeselected?.Invoke();
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

            OnObjectSelected?.Invoke();
        }

        #endregion
        #region Component Editor
        Grid addComponentGrid;
        bool addComponentMenuOpen = false;
        private Dictionary<string, Func<Component>> addComponentFunctions;
        private List<Action> addComponentActions = new();
        public static List<Action> editComponentActions = new();

        /// <summary>
        /// Todo: figure out why this dupes the inspector when it's open, It should always just refresh it entirely.
        /// </summary>
        /// <param name="grid"></param>
        private void RefreshInspector()
        {
            itemsControl.Items.Clear();
            activeControls.Clear();
            editComponentActions.Clear();
            if (lastSelectedNode == null)
                return;
            itemsControl.Items.Add(TransformHeader());
            foreach (var componentType in lastSelectedNode.Components.Values)
                foreach (var component in componentType)
                    itemsControl.Items.Add(ComponentHeader(component));
            var addComponentButton = Inspector.GetButton("Add Component", new(0, 0, 0, 0));
            addComponentButton.ContextMenu = Editor.Current.FindResource("ContextMenu") as ContextMenu;
            addComponentButton.Click += AddComponentButton_Click;
            itemsControl.Items.Add(addComponentButton);
            OnInspectorUpdated?.Invoke();
        }
        private Grid TransformHeader()
        {
            TransformComponent transform = new()
            {
                node = lastSelectedNode,
                position = lastSelectedNode.Position,
                scale = lastSelectedNode.Scale,
                rotation = lastSelectedNode.Rotation
            };
            return ComponentHeader(transform, false, lastSelectedNode.Name);
        }
        private Grid ComponentHeader(Component component, bool removable = true, string? name = null)
        {
            Grid memberGrid = new();
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            var box = GetTextBox(name ?? component.GetType().Name);
            Grid.SetColumn(box, 0);
            memberGrid.Children.Add(box);

            Button editComponentButton = GetButton("Edit", new(0, 0, 0, 0));
            editComponentButton.Click += (s, e) =>
            {
                Editor.Current.componentEditor ??= new();
                Editor.Current.componentEditor.Dispose();
                Editor.Current.componentEditor.Refresh(component);
            };
            Grid.SetColumn(editComponentButton, 1);
            memberGrid.Children.Add(editComponentButton);
            if (removable)
            {
                Button removeButton = GetRemoveComponentButton(component);
                Grid.SetColumn(removeButton, 2);
                memberGrid.Children.Add(removeButton);
            }
            return memberGrid;
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

            if (sender is not MenuItem menuItem) return;
            if (menuItem.Name.ToInt() is int i && addComponentActions.Count > i)
                addComponentActions[i]?.Invoke();
        }
        private void RefreshAddComponentFunctions()
        {
            addComponentFunctions ??= new();
            addComponentFunctions.Clear();

            var types = Runtime.AllComponents;
            foreach (var type in types)
            {

                var attr = type.GetCustomAttribute(typeof(HideFromEditorAttribute));

                if (attr is not null)
                    continue; 

                if (!addComponentFunctions.ContainsKey(type.Name))
                    InstantiateAndAddComponent(type);
            }
        }
        private void InstantiateAndAddComponent(Type type)
        {
            addComponentFunctions.Add(type.Name, () =>
            {
                return lastSelectedNode.AddComponent(type);
            });
        }
        private void AddComponentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.ContextMenu is not ContextMenu menu)
                return;
            e.Handled = true;

            RefreshAddComponentFunctions();
            menu.Items.Clear();

            int i = 0;
            foreach (var item in addComponentFunctions)
            {
                addComponentActions.Add(() => AddComponent(new(item.Key, item.Value)));
                MenuItem menuItem = new MenuItem { Header = item.Key };
                menuItem.Name = $"button{i}";
                menuItem.Click += AddComponentClicked;
                menu.Items.Add(menuItem);
                i++;
            }

            menu.IsOpen = true;
        }

        #endregion
        #region UI Function
        
        private List<Control> activeControls = new();
        private ItemsControl itemsControl;
        public static TextBox GetTextBox(string text, string style = "default")
        {
            return style switch
            {
                "default" => DefaultStyle(text),
                "mint" => MintStyle(text),
                _ => DefaultStyle(text),
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
        private static TextBox DefaultStyle(string text)
        {
            return new()
            {
                Text = text,
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
        private static Button GetRemoveComponentButton(Component component)
        {
            Button editComponentButton = GetButton("Remove", new(0, 0, 0, 0));
            editComponentButton.Click += (_, e) =>
            {
                e.Handled = true; 
                component.RemoveComponent(component);
            };
            editComponentButton.Name = "remove_button_" + component.GetType().Name;
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
        public static event Action? OnObjectSelected;
        public static event Action? OnObjectDeselected;
        public static event Action? OnInspectorUpdated;
        public static event Action? OnComponentAdded;
        public static event Action? OnComponentRemoved;
        public static Action<int, int>? OnInspectorMoved;

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

        #endregion
    }
}