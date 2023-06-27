using Pixel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Reflection;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using Component = Pixel.Types.Components.Component;
using static Pixel.Input;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Pixel_Editor
{
    public partial class InspectorControl : UserControl
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
        object editorCollectionLockObj = new object();
        public InspectorControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            BindingOperations.EnableCollectionSynchronization(ComponentEditors, editorCollectionLockObj);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OnObjectSelected += RefreshInspector;
            OnObjectDeselected += RefreshInspector;
            OnComponentAdded += RefreshInspector;
            OnComponentRemoved += RefreshInspector;
            OnInspectorMoved += Inspector_OnInspectorMoved;
            AddComponentCommand.Action += AddComponentButton_Click;
            RefreshInspectorsAction += RefreshInspector;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            OnObjectSelected -= RefreshInspector;
            OnObjectDeselected -= RefreshInspector;
            OnComponentAdded -= RefreshInspector;
            OnComponentRemoved -= RefreshInspector;
            OnInspectorMoved -= Inspector_OnInspectorMoved;
            AddComponentCommand.Action -= AddComponentButton_Click;
            RefreshInspectorsAction -= RefreshInspector;
        }
        #region Node Function
        public static void DeselectNode()
        {
            if (Editor.Current.LastSelected != null)
            {
                Editor.Current.LastSelected = null;
                OnObjectDeselected?.Invoke();
            }
            Runtime.Current.stagingHost.DeselectNode();
            Editor.Current.LastSelected = null;
        }
        public static void SelectNode(Node node)
        {
            Editor.Current.LastSelected = node;
            if (node != null)
                foreach (var comp in node.Components)
                    foreach (var component in comp.Value) component.selected_by_editor = true;

            OnObjectSelected?.Invoke();
        }
        static Action? RefreshInspectorsAction;

        #endregion
        #region Component Editor
        public ObservableCollection<ComponentEditor> ComponentEditors { get; } = new();
        public ObservableProperty<Visibility> AddComponentVisibility { get; } = new(Visibility.Hidden);
        public ActionCommand AddComponentCommand { get; } = new();
        private Dictionary<string, Func<Component>> addComponentFunctions = new();
        private List<Action> addComponentActions = new();
        public static void RefreshInspectors() => RefreshInspectorsAction?.Invoke();
        public void RefreshInspector()
        {
            ComponentEditors.Clear();
            if (Editor.Current.LastSelected is not Node selectedNode)
            {
                AddComponentVisibility.Value = Visibility.Hidden;
                return;
            }
            AddComponentVisibility.Value = Visibility.Visible;
            ComponentEditors.Add(TransformHeader(selectedNode));
            foreach (var componentType in selectedNode.Components.Values)
                foreach (var component in componentType)
                    ComponentEditors.Add(ComponentHeader(component));
            OnInspectorUpdated?.Invoke();
        }
        private ComponentEditor TransformHeader(Node selectedNode)
        {
            TransformComponent transform = new()
            {
                name = selectedNode.Name,
                node = selectedNode,
                position = selectedNode.Position,
                scale = selectedNode.Scale,
                rotation = selectedNode.Rotation
            };
            return ComponentHeader(transform, false, selectedNode.Name);
        }
        private ComponentEditor ComponentHeader(Component component, bool removable = true, string? name = null)
        {
            ComponentEditor componentEditor = new();
            componentEditor.Name = name ?? component.Name;
            componentEditor.Control.Refresh(component);
            componentEditor.EditCommand.Action = (s) => componentEditor.Visible = !componentEditor.Visible;
            if (removable)
                componentEditor.RemoveCommand.Action = (s) =>
                {
                    component.RemoveComponent(component);
                    RefreshInspectors();
                };
            return componentEditor;
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
            RefreshInspectors();
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
            if (Editor.Current.LastSelected is null)
                return;

            addComponentFunctions.Add(type.Name, () =>
            {
                return Editor.Current.LastSelected.AddComponent(type);
            });
        }
        private void AddComponentButton_Click(object? sender)
        {
            if (sender is not Button button || button.ContextMenu is not ContextMenu menu)
                return;

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
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 30, 30, 30)),

            };
        }
        public static Button GetButton(string content, Thickness margin) => new Button()
        {
            Content = content,
            Margin = margin,
            BorderThickness = new Thickness(0.1, 0.1, 0.1, 0.1),
        };
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
    public class ComponentEditor : INotifyPropertyChanged
    {
        private string name = "";
        private bool visible = false;
        private ComponentEditorControl control = new();
        public ActionCommand EditCommand { get; private set; } = new();
        public ActionCommand RemoveCommand { get; private set; } = new();
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                PropertyChanged?.Invoke(this, new(nameof(Visible)));
            }
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
        public ComponentEditorControl Control
        {
            get => control;
            set
            {
                control = value;
                PropertyChanged?.Invoke(this, new(nameof(Control)));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed; // Default value if not a bool
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
