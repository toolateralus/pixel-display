using Pixel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Pixel_Editor
{
    /// <summary>
    /// Interaction logic for StageViewerWindow.xaml
    /// </summary>
    public partial class StageViewerWindow : UserControl
    {
        public static BitmapImage SourceImage { get; set; } = new();
        public StageViewerWindow()
        {
            InitializeComponent();
            DataContext = this;
            Editor.Current.mainImage = image;
            Runtime.SetOutputImageAsMain(image);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e) => Editor.Current.input.MouseDown?.Invoke(this, e);
        protected override void OnMouseUp(MouseButtonEventArgs e) => Editor.Current.input.MouseUp?.Invoke(this, e);
        protected override void OnKeyDown(KeyEventArgs e) => Editor.Current.input.KeyDown?.Invoke(this, e);
        protected override void OnKeyUp(KeyEventArgs e) => Editor.Current.input.KeyUp?.Invoke(this, e);
        protected override void OnMouseEnter(MouseEventArgs e) => Editor.Current.input.MouseEnter?.Invoke(this, e);
        protected override void OnMouseLeave(MouseEventArgs e) => Editor.Current.input.MouseLeave?.Invoke(this, e);
        protected override void OnMouseMove(MouseEventArgs e) => Editor.Current.input.MouseMove?.Invoke(this, e);
        protected override void OnMouseWheel(MouseWheelEventArgs e) => Editor.Current.input.MouseWheel?.Invoke(this, e);
    }
}
