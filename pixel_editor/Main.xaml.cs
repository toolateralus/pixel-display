using System;

using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;

using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace pixel_editor
{
    public partial class Editor : Window
    {
        #region Window Scaling
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(Editor), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));
        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            Editor mainWindow = o as Editor;
            return mainWindow != null ? mainWindow.OnCoerceScaleValue((double)value) : value;
        }
        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Editor mainWindow = o as Editor;
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

        internal EngineInstance? engine;
        internal RenderHost? host => Runtime.Instance.renderHost;  

        private readonly Inspector inspector;
        private StageWnd? stageWnd;
        private int renderStateIndex = 0;
        public Editor()
        {
            InitializeComponent();
            inspector = new Inspector(inspectorObjName,  inspectorObjInfo,  inspectorChildGrid);
            Runtime.inspector = inspector;
            engine = new();
            GetEvents(); 
        }


        private void GetEvents()
        {
           // HandleInputMediator();
            CompositionTarget.Rendering += Update;
            Closing += OnDisable;
            image.MouseLeftButtonDown += Mouse0;
        }
        
        private InputMediator input; 
        Key[] keys = new Key[]
        {
            Key.F1,
            Key.F2,
            Key.F3,
            Key.F4,
            Key.F5,
        };
        private void HandleInputMediator()
        {
            Action[] inputActions = new Action[]
            {
                () => { Runtime.Instance.Toggle(); },
            };
            Key[] inputs = new Key[]
            {
                Key.F1,
            };

            input = new(inputActions, inputs);
        }

        private void IncrementRenderState()
        {
            if (Runtime.Instance.stage is null)
            {
                host.State = RenderState.Off;
                viewBtn.Content = "Stage null.";
            }
            renderStateIndex++;
            if (renderStateIndex == sizeof(RenderState) - 1)
            {
                renderStateIndex = 0;
            }
            host.State = (RenderState)renderStateIndex;
            viewBtn.Content = host.State.ToString();
            if (host.State == RenderState.Game)
            {
                var msg = MessageBox.Show("Enter Game View?", "Game View", MessageBoxButton.YesNo);
                if (msg != MessageBoxResult.Yes)
                    return;
                Runtime.Instance.mainWnd = new();
                Runtime.Instance.mainWnd.Show();
            }
        }
        private void Update(object? sender, EventArgs e)
        {
            inspector.Update(sender, e);

            if (Runtime.Instance.IsRunning 
                && Runtime.Instance.stage is not null  
                && host.State == RenderState.Scene)
                {
                    host.Render(image);
                    gcAllocText.Content =
                        $"{Runtime.Instance.renderHost.info.GetTotalMemory()}" +
                        $" \n frame rate : {Runtime.Instance.renderHost.info.Framerate}"; 
                }

            // Immediately stop reading input because the method in the body of the
            // case will get called dozens of times while app is in break mode w/ debugger without it.

            foreach (var key in keys)
                switch (key)
                {
                    case Key.F1:
                        if (Input.GetKeyDown(key))
                        {
                            Input.SetKey(key, false);
                            Runtime.Instance.TrySetStageAsset(0);
                        }
                        break;                                                  
                    case Key.F2:                                             
                        if (Input.GetKeyDown(key))
                        {
                            Input.SetKey(key, false);
                            if(Runtime.Instance.GetStageAsset() != null)
                                Runtime.Instance.stage = Runtime.Instance.stage.Reset();
                        }
                        break;                                                  
                    case Key.F3:                                             
                        if (Input.GetKeyDown(key))
                        {
                            Input.SetKey(key, false);
                            Runtime.Instance.TrySetStageAsset(2);
                        }
                        break;                                               
                    case Key.F4:                                              
                        if (Input.GetKeyDown(key))
                        {
                            Input.SetKey(key, false);
                            Runtime.Instance.TrySetStageAsset(3);
                        }
                        break;                                                  
                    case Key.F5:                                             
                        if (Input.GetKeyDown(key))
                        {
                            Input.SetKey(key, false);
                            Runtime.Instance.TrySetStageAsset(4);
                        }
                        break;
                }
        }
        private void Wnd_Closed(object? sender, EventArgs e) =>
            stageWnd = null;    


        // WPF Control Events.
        private void Mouse0(object sender, MouseButtonEventArgs e)
        {
            // this cast could be causing erroneous behavior
            Point pos = e.GetPosition((Image)sender);

            if (Runtime.Instance.IsRunning)
            {
                inspector.DeselectNode();
                if (Runtime.Instance.stagingHost.GetNodeAtPoint(Runtime.Instance.stage, pos, out Node node))
                    inspector.SelectNode(node);
            }
        }
        private void OnDisable(object? sender, EventArgs e)
        {
            stageWnd?.Close(); 
            engine?.Close();
        }
        private void OnPlay(object sender, RoutedEventArgs e)
        {
            Runtime.Instance.Toggle();  
            if (Runtime.Instance.IsRunning)
            {
                playBtn.Background = Brushes.LightGreen;
            }
            else playBtn.Background = Brushes.LightPink; 
        }
        private void OnViewChanged(object sender, RoutedEventArgs e) => IncrementRenderState();
        private void OnImportBtnPressed(object sender, RoutedEventArgs e) => _ = Importer.ImportAsync(true);
        private void OnSyncBtnPressed(object sender, RoutedEventArgs e)
        {
            ProjectIO.SaveProject(Runtime.Instance.LoadedProject);
            Library.Sync();
        }
        private void OnStagePressed(object sender, RoutedEventArgs e)
        {
            stageWnd = new StageWnd(this);
            stageWnd.Show();
            stageWnd.Closed += Wnd_Closed;
        }
        private void OnImportFileButtonPressed(object sender, RoutedEventArgs e)
        {
            Importer.ImportAssetDialog();
        }
        private void OnLoadProjectPressed(object sender, RoutedEventArgs e)
        {
            Project project = Project.LoadProject();
            if (project is not null)
                Runtime.Instance.SetProject(project);
        }
    }
    /// <summary>
    ///  not done
    /// </summary>
    internal class InputMediator
    {
        public Dictionary<Key, Action> InputEvents = new Dictionary<Key, Action>();

        public InputMediator(Action[] events, Key[] keys)
        {
            foreach (var _key in keys)
                foreach (var _event in events)
                {
                    InputEvents.Add(_key, _event);
                    Input.SetKey(_key, false);
                }

            CompositionTarget.Rendering += Update;
        }

        private void Update(object? sender, EventArgs e)
        {
            foreach (var key in InputEvents.Keys)
                if (Input.GetKeyDown(key))
                    ExecuteEvents(InputEvents[key]); 
        } 
        public static void ExecuteEvents(Action action) =>action.Invoke();
    }
}