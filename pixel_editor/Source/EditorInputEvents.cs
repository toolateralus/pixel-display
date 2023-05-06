using System;
using System.Windows;
using System.Windows.Input;

namespace Pixel_Editor
{
    public class EditorInputEvents
    {
        public Action<object?, MouseButtonEventArgs>? MouseDown;
        public Action<object?, MouseButtonEventArgs>? MouseUp;
        public Action<object?, KeyEventArgs>? KeyDown;
        public Action<object?, KeyEventArgs>? KeyUp;
        public Action<object?, MouseEventArgs>? MouseEnter;
        public Action<object?, MouseEventArgs>? MouseLeave;
        public Action<object?, MouseEventArgs>? MouseMove;
        public Action<object?, MouseWheelEventArgs>? MouseWheel;
    }
}
