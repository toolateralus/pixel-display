using Pixel;
using System.Reflection;
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
        public StageViewerWindow()
        {
            InitializeComponent();
            DataContext = this;
            Editor.Current.mainImage = image;
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
    }
}
