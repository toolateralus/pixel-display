using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Windows;
using System.Linq;
using System.Runtime.CompilerServices;

namespace pixel_renderer
{
    public class CMouse
    {
        public static bool Left;
        public static bool LeftPressedLastFrame;
        public static bool LeftPressedThisFrame;
        public static bool Middle;
        public static bool Right;
        public static bool XButton1;
        public static bool XButton2;
        
        public static Vec2 Position;
        public static Vec2 GlobalPosition;
        public static Vec2 LastPosition { get; set; }
        public static Vec2 Delta { get { return LastPosition - Position; } }

        private static CMouse current = null; 
        public static CMouse Current
        {
            get
            {
                current ??= new();
                return current;
            }
        }

        public static int MouseWheelDelta { get; set; }

        public CMouse(MouseButtonEventArgs? e = null)
        {
            if (current != this)
                current = null;

            current = this;

            if(e is not null)
                Refresh(e);
        }

        public static void Refresh(MouseButtonEventArgs e)
        {
            e.Handled = true;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    Left = e.LeftButton is MouseButtonState.Pressed;
                    break;
                case MouseButton.Middle:
                    Middle = e.MiddleButton is MouseButtonState.Pressed;
                    break;
                case MouseButton.Right:
                    Right = e.RightButton is MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton1:
                    XButton1 = e.XButton1 is MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton2:
                    XButton2 = e.XButton2 is MouseButtonState.Pressed;
                    break;
            }

          
        }
        public static void Refresh(MouseWheelEventArgs e)
        {
            e.Handled = true;
            MouseWheelDelta = e.Delta;
        }
        public static void Refresh(MouseEventArgs e)
        {
            e.Handled = true; 
            LastPosition = Position;
            var img = Runtime.OutputImages.First();
            var point = e.GetPosition(img);
            var normalizedPos = (Vec2)img.GetNormalizedPoint(point);
            GlobalPosition = Camera.First.ScreenViewportToGlobal(normalizedPos);
            Position = (Vec2)point;
        }
    }

    public enum InputEventType { KeyDown, KeyUp, KeyToggle }
    public static class Input
    {


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
                    bool input_value = GetInputValueByType(ref action.Key, type);
                    if (input_value)
                        if (action.ExecuteAsynchronously) Task.Run(() => action.InvokeAsync());
                        else action.Invoke();
                }
            }
        }
        public static bool GetInputValue(InputEventType type, string key)
        {
            return Application.Current.Dispatcher.Invoke(() => { 
            Key key_ = Enum.Parse<Key>(key);
            var input_value = type switch
            {
                InputEventType.KeyDown => Keyboard.IsKeyDown(key_),
                InputEventType.KeyUp => Keyboard.IsKeyUp(key_),
                InputEventType.KeyToggle => Keyboard.IsKeyToggled(key_),
                _ => false,
            };
            return input_value;
            });
        }

        public static bool GetInputValue(Key key, InputEventType type = InputEventType.KeyDown)
        {
            return Application.Current.Dispatcher.Invoke(() => {
                var input_value = type switch
                {
                    InputEventType.KeyDown => Keyboard.IsKeyDown(key),
                    InputEventType.KeyUp => Keyboard.IsKeyUp(key),
                    InputEventType.KeyToggle => Keyboard.IsKeyToggled(key),
                    _ => false,
                };
                return input_value;
            });
        }
        public static bool GetInputValueByType(ref Key key, InputEventType type = InputEventType.KeyDown)
        {
            return GetInputValue(key, type);
        }
        public static void RegisterAction(Action<object[]?> action, Key key, InputEventType type = InputEventType.KeyDown)
        {
             InputActions.Add(new(action, key, type: type));
        }
    }

    public class InputAction
    {
        internal Key Key;
        internal InputEventType EventType = InputEventType.KeyDown; 
        internal readonly bool ExecuteAsynchronously = false;
        private ValueTuple<Action<object[]?>, object[]?> Action_Args = new();

        public InputAction(Action<object[]?> expression, Key key, object[]? args = null, bool async = false, InputEventType type = InputEventType.KeyDown)
        {
            ExecuteAsynchronously = async;
            Action_Args.Item1 = expression;
            Action_Args.Item2 = args;
            Key = key;
            EventType = type;
        }
        internal void Invoke() => Action_Args.Item1?.Invoke(Action_Args.Item2);
        internal async Task InvokeAsync(float? delay = null)
        {
            if (delay is not null)
                await Task.Delay((int)delay);
             await Task.Run(() => Action_Args.Item1?.Invoke(Action_Args.Item2));
        }
    }
}