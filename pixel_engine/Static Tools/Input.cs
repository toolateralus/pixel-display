using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pixel.Types;
using Pixel.Types.Components;
using PixelLang.Libraries.Input;

namespace Pixel
{
    public static partial class Input
    {
        [LibraryImport("libX11.so.6")]
        private static partial nint XOpenDisplay(nint display);
        [LibraryImport("libX11.so.6")]
        private static partial void XCloseDisplay(nint display);
        [LibraryImport("libX11.so.6")]
        private static partial int XPending(nint display);
        [LibraryImport("libX11.so.6")]
        private static partial int XNextEvent(nint display, ref XEvent e);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct XEvent
        {
            public int type;
            public nint xany;
        }
        
        private const int KeyPress = 2;
        private const int KeyRelease = 2;
        private const int MotionNotify = 2;
        private static nint display;
        static Input()
        {
            display = XOpenDisplay(nint.Zero);
            if(display == nint.Zero)
            {
                throw new NullReferenceException("Failed to open display in input");
            }
        }
        
        private static readonly List<InputAction> InputActions = new(250);
        internal static void Refresh()
        {
            lock (InputActions)
            {
                InputAction[] actions = new InputAction[InputActions.Count];
                InputActions.CopyTo(0, actions, 0, InputActions.Count); 
                foreach (var action in actions)
                {
                    var type = action.EventType;
                    bool input_value = Get(action.Key, type);
                    if (input_value)
                        if (action.ExecuteAsynchronously) Task.Run(() => action.InvokeAsync());
                        else action.Invoke();
                }
            }
        }
        public static bool Get(string key, InputEventType type = InputEventType.KeyDown)
        {
            if (XPending(display) > 0)
            {
                XEvent e = new();
                XNextEvent(display, ref e);
                
                if (e.type == KeyPress)
                {
                    // todo: define xany event struct so we can get
                    // the key's serial number to verify what was
                    // pressed/moved.
                }
            }
            return false;
        }
        public static bool Get(Key key, InputEventType type = InputEventType.KeyDown)
        {
            return false;
        }
        public static void RegisterAction(object caller, Action action, Key key, InputEventType type = InputEventType.KeyDown)
        {
            InputAction item = new(action, key, type: type);
            if (caller is Component c) c.node.OnDestroyed += () => InputActions.Remove(item);
            if (caller is Node n) n.OnDestroyed += () => InputActions.Remove(item);
            if (caller is Action a) a += () => InputActions.Remove(item);
            InputActions.Add(item);
        }
    }
}