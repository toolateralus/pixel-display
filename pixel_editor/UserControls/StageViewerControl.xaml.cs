using Pixel;
using PixelLang.Tools;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel_Editor
{
    public partial class StageViewerControl : UserControl, INotifyPropertyChanged
    {
        SolidColorBrush framerateBrush = Brushes.White;
        public SolidColorBrush FramerateBrush
        {
            get => framerateBrush;
            set
            {
                framerateBrush = value;
                OnPropertyChanged(nameof(FramerateBrush));
            }
        }
        string framerateLabel = "";
        public string FramerateLabel
        {
            get => framerateLabel;
            set
            {
                framerateLabel = value;
                OnPropertyChanged(nameof(FramerateLabel));
            }
        }
        public StageViewerControl()
        {
            InitializeComponent();
            DataContext = this;
            Editor.Current.mainImage = image;
            Editor.EditorUpdate += UpdateMetrics;
            Runtime.SetOutputImageAsMain(image);
            image.PreviewMouseDown += (s, e) => Editor.Current.input.MouseDown?.Invoke(s, e);
            image.PreviewMouseUp += (s, e) => Editor.Current.input.MouseUp?.Invoke(s, e);
            image.PreviewKeyDown += (s, e) => Editor.Current.input.KeyDown?.Invoke(s, e);
            image.PreviewKeyUp += (s, e) => Editor.Current.input.KeyUp?.Invoke(s, e);
            image.MouseEnter += (s, e) => Editor.Current.input.MouseEnter?.Invoke(s, e);
            image.MouseLeave += (s, e) => Editor.Current.input.MouseLeave?.Invoke(s, e);
            image.PreviewMouseMove += (s, e) => Editor.Current.input.MouseMove?.Invoke(s, e);
            image.PreviewMouseWheel += (s, e) => Editor.Current.input.MouseWheel?.Invoke(s, e);
        }
        private void UpdateMetrics()
        {
            RenderHost renderHost = Runtime.Current.renderHost;
            RenderInfo info = renderHost.info;

            if (info.frameCount % 60 == 0)
            {
                var framerate = info.Framerate;
                var min = info.lowestFrameRate;
                var max = info.highestFrameRate;
                var avg = info.averageFrameRate;

                switch (avg)
                {
                    case < 10:
                        FramerateBrush = Brushes.DarkRed;
                        break;
                    case < 20:
                        FramerateBrush = Brushes.Red;
                        break;
                    case < 30:
                        FramerateBrush = Brushes.DarkOrange;
                        break;
                    case < 40:
                        FramerateBrush = Brushes.Orange;
                        break;
                    case < 50:
                        FramerateBrush = Brushes.Yellow;
                        break;
                    case < 60:
                        FramerateBrush = Brushes.White;
                        break;
                    case >= 60:
                        FramerateBrush = Brushes.Green;
                        break;
                }
                FramerateLabel = $"last : {framerate} avg :{avg}\n min : {min} max :{max}";
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
