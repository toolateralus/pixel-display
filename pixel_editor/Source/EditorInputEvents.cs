using System;
using System.Windows.Input;

namespace Pixel_Editor
{
    public class EditorInputEvents
    {
        public Action<StageViewerWindow, MouseButtonEventArgs>? MouseDown;
        public Action<StageViewerWindow, MouseButtonEventArgs>? MouseUp;
        public Action<StageViewerWindow, KeyEventArgs>? KeyDown;
        public Action<StageViewerWindow, KeyEventArgs>? KeyUp;
        public Action<StageViewerWindow, MouseEventArgs>? MouseEnter;
        public Action<StageViewerWindow, MouseEventArgs>? MouseLeave;
        public Action<StageViewerWindow, MouseEventArgs>? MouseMove;
        public Action<StageViewerWindow, MouseWheelEventArgs>? MouseWheel;
    }
}
