#region Reassigned
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;
#endregion 
#region Usings
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using System.Windows.Input;
using pixel_renderer;
#endregion

namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Main : Window
    {
        static EngineInstance engine; 
        Inspector inspector;
        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        private static int renderStateIndex = 0; 
        // main entry point for application
        public Main()
        {
            InitializeComponent();
            engine = new();
            engine.Show();
            inspector = new Inspector(inspectorObjName,
                                      inspectorObjInfo,
                                      inspectorChildGrid);
            GetEvents();
        }

        private void GetEvents()
        {
            CompositionTarget.Rendering += Update;
            Closing += OnDisable;
            image.MouseLeftButtonDown += Mouse0;
        }
        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);
            if (!Runtime.Instance.running 
                || Rendering.State != RenderState.Scene) return; 

            Rendering.Render(image); 
        }
        /// <summary>
        /// Called on program close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisable(object? sender, EventArgs e)
        {
            engine.Close();
        }
        private void OnViewChanged(object sender, RoutedEventArgs e)
        {
            IncrementRenderState();
        }
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            if (!Runtime.Instance.running)
            {
                engine.Accept_Clicked(sender, e); 
            }
        }
        private void IncrementRenderState()
        {
            renderStateIndex++;
            if (renderStateIndex == sizeof(RenderState))
            {
                renderStateIndex = 0;
            }
            Rendering.State = (RenderState)renderStateIndex;
            viewBtn.Content = Rendering.State.ToString();
        }
        private void Mouse0(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(sender as Image);
            if (Runtime.Instance.running)
            {
                inspector.DeselectNode();
                if (Staging.TryCheckOccupant(pos, out Node node))
                {
                    inspector.SelectNode(node);
                }
            }
        }

        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(Main), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged),new CoerceValueCallback(OnCoerceScaleValue)));
        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            Main mainWindow = o as Main;
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else return value;
        }
        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Main mainWindow = o as Main;
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }
        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);
            return value;
        }
        protected virtual void OnScaleValueChanged(double oldValue, double newValue) { }
        private void CalculateScale()
        {
            double yScale = ActualHeight / 250f;
            double xScale = ActualWidth / 200f;
            double value = Math.Min(xScale, yScale);

            ScaleValue = (double)OnCoerceScaleValue(window, value);
        }
        public void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale();
        }
        #endregion

    }
    public class Inspector
    {
        public Inspector(Label name, Label objInfo, Grid componentGrid)
        {
            this.name = name ?? new();
            this.objInfo = objInfo ?? new();
            this.componentGrid = componentGrid ?? new();

            this.name.Content = "Object Name";
            this.objInfo.Content = "Object Info";
            // TODO : implement awake event or message call, not calling a method xD

            Awake(); 
        }
        public Node? loadedNode;
        private List<TextBlock> currentInspector = new(); 
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
        public void Update(object? sender, EventArgs e)
        {
     
        }
        /// <summary>
        /// If a Node is loaded, get Node and Component info and assemble a UI based on gathered info.
        /// </summary>
        private void Refresh()
        {
            if (loadedNode == null) return;
            name.Content = loadedNode.Name;
            objInfo.Content = $"{loadedNode.Components.Count} Components";
            components = loadedNode.Components;
            var thickness = new Thickness(0, 0, 0, 0);
            
            int index = 0;
            foreach (var componentType in components.Values)
            {
                foreach (var component in componentType)
                {
                    string info = GetComponentInfo(component);
                    TextBlock block = CreateBlock(info, thickness);
                    AddToInspector(index, block, info.Split(' ').Length);
                    componentGrid.Children.Add(block);
                    componentGrid.UpdateLayout();
                    currentInspector.Add(block);
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
                foreach (var control in currentInspector)
                {
                    componentGrid.Children.Remove(control);
                }
                currentInspector.Clear(); 
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
            component.SetValue(Grid.RowSpanProperty, rowSpan - 3);
            component.SetValue(Grid.ColumnSpanProperty, 8);
            component.SetValue(Grid.RowProperty, i * 2);
            component.SetValue(Grid.ColumnProperty, 6);
        }
        public static string GetComponentInfo(Component component)
        {
            IEnumerable<PropertyInfo> properties = component.GetType().GetRuntimeProperties();
            IEnumerable<FieldInfo> fields = component.GetType().GetRuntimeFields();
            string output = $"\b {component.GetType().Name} Properties : \n";
            // todo = add field and property values to an editable text box aside the label,
            // once changed send event to update property accordingly.
            // todo = make individual labels / text boxes for each field / property
            foreach (var property in properties)
            {
                output += $"\t{property.Name} {property.PropertyType}\n";
            }
            foreach (var field in fields)
            {
                output += $"\t{field}\n";
            }
            return output;
        }
        
        public static Label CreateLabel(string componentInfo, Thickness margin)
        {
            margin.Bottom = componentInfo.Split(' ').Length;
            return new Label
            {
                Content = componentInfo,
                FontSize = 2.25f,
                Background = Brushes.DarkGray,
                Foreground = Brushes.White, 
                Margin = margin,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic")
            };
        }
        public static TextBlock CreateBlock(string componentInfo, Thickness margin)
        {
            margin.Bottom = componentInfo.Split(' ').Length + 5;
            return new TextBlock()
            {
                Text = componentInfo,
                FontSize = 2.25f,
                FontFamily = new System.Windows.Media.FontFamily("MS Gothic"),
                TextWrapping = TextWrapping.Wrap,
                Background = Brushes.DarkGray,
                Foreground = Brushes.White,
                
                Margin = margin,
                
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
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