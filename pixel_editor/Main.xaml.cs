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
        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(Main), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));
        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            Main mainWindow = o as Main;
            return mainWindow != null ? mainWindow.OnCoerceScaleValue((double)value) : value;
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
        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        #endregion

        Inspector inspector;
        /// <summary>
        /// keep this reference of the engine just to close the background window on editor exit.
        /// </summary>
        internal static EngineInstance? engine;
        private static int renderStateIndex = 0;
        private StageWnd stageWindow;

        // main entry point for application
        public Main()
        {
            InitializeComponent();
            inspector = new Inspector(inspectorObjName,
                                      inspectorObjInfo,
                                      inspectorChildGrid);
            Runtime.inspector = inspector;
            engine = new();
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
            if (Runtime.Instance.IsRunning && Runtime.Instance.stage is not null
                && Rendering.State == RenderState.Scene)
            {
                Rendering.Render(image);
                gcAllocText.Content = Rendering.GetGCStats();
            }
        }
        /// <summary>
        /// Called on program close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisable(object? sender, EventArgs e) => engine.Close();
        private void OnViewChanged(object sender, RoutedEventArgs e) => IncrementRenderState();
        private void OnPlay(object sender, RoutedEventArgs e) => Runtime.Instance.Toggle();
        private void IncrementRenderState()
        {
            if (Runtime.Instance.stage is null)
            {
                Rendering.State = RenderState.Off;
                viewBtn.Content = "Stage null.";
            }
            renderStateIndex++;
            if (renderStateIndex == sizeof(RenderState) - 1)
            {
                renderStateIndex = 0;
            }
            Rendering.State = (RenderState)renderStateIndex;
            viewBtn.Content = Rendering.State.ToString();
            if (Rendering.State == RenderState.Game)
            {
                var msg = MessageBox.Show("Enter Game View?", "Game View", MessageBoxButton.YesNo);
                if (msg != MessageBoxResult.Yes)
                    return;
                Runtime.Instance.mainWnd = new();
                Runtime.Instance.mainWnd.Show();
            }
        }
        private void Mouse0(object sender, MouseButtonEventArgs e)
        {
            // this cast could be causing erroneous behavior
            Point pos = e.GetPosition((Image)sender);

            if (Runtime.Instance.IsRunning)
            {
                inspector.DeselectNode();
                if (Staging.TryCheckOccupant(pos, out Node node))
                    inspector.SelectNode(node);
            }
        }
        private void OnImportBtnPressed(object sender, RoutedEventArgs e) => AssetPipeline.ImportAsync(true);
        private void OnSyncBtnPressed(object sender, RoutedEventArgs e) => AssetLibrary.Sync();
        private void OnImportFileButtonPressed(object sender, RoutedEventArgs e)
        {
            AssetPipeline.ImportFileDialog();
        }
        private void OnStagePressed(object sender, RoutedEventArgs e)
        {
            stageWindow = new StageWnd();
            stageWindow.Show();
            stageWindow.Closed += Wnd_Closed;

        }
        private void Wnd_Closed(object? sender, EventArgs e) =>
            stageWindow = null;

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
        private void Instance_InspectorEventRaised(InspectorEvent e)
        {
            Runtime.Instance.IsRunning = false; 
            var msg = MessageBox.Show(e.expression.ToString(), e.message, MessageBoxButton.YesNo);
            var args = e.expressionArgs;
            if (args.Length < 4) return;
            e.expression(args[0], args[1] ?? null, args[2] ?? null, args[3] ?? null);
        }

        public void Update(object? sender, EventArgs e)
        {

        }
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
            IEnumerable<FieldInfo> fields = component.GetType().GetRuntimeFields();
            string output = $"\b {component.GetType().Name} Properties : ";

            IEnumerable<PropertyInfo> properties = component.GetType().GetRuntimeProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(component, null);
                output += $" \n \t{property.Name} {property.PropertyType} {value}";
            }

            foreach (var field in fields)
            {
                var value = field.GetValue(component);
                output += $" \n \t{field} {value}";
            }
            return output;
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
    }
}