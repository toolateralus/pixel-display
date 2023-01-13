using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pixel_renderer
{

    public static class Input
    {
        public enum InputEventType { DOWN, UP, TOGGLE }

        /// 101 keys on US Standard Keyboard.
        static List<InputAction> InputActions_KeyDown = new(101);
        static List<InputAction> InputActions_KeyUp = new(101);
        static List<InputAction> InputActions_KeyToggled = new(101);
        
        public static event Action<Key>? OnKeyDown;
        public static event Action<Key>? OnKeyUp;
        public static event Action<Key>? OnKeyToggled;

        internal static void Awake()
        {
            OnKeyDown += Input_OnKeyDown;
            OnKeyUp += Input_OnKeyUp;
            OnKeyToggled += Input_OnKeyToggled;
        }
       
        public static void RegisterAction(bool async, Action<object[]?> action, object[]? args, Key key, InputEventType type)
        {
            InputAction ia = new(async, action, args, key);
            RegisterAction(ia, type);
        }
        public static void RegisterAction(InputAction action, InputEventType type)
        {
            switch (type)
            {
                case InputEventType.DOWN:
                    InputActions_KeyDown.Add(action);
                    break;
                case InputEventType.UP:
                    InputActions_KeyUp.Add(action);
                    break;
                case InputEventType.TOGGLE:
                    InputActions_KeyToggled.Add(action);
                    break;
            }
        }

        private static void Input_OnKeyToggled(Key key)
        {
            foreach (InputAction action in InputActions_KeyToggled)
                if (action.Key.Equals(key))
                {
                    if (action.ExecuteAsynchronously)
                    {
                        action?.InvokeAsync();
                        return;
                    }
                    action?.Invoke();
                }
        }
        private static void Input_OnKeyUp(Key key)
        {
            foreach (InputAction action in InputActions_KeyUp)
                if (action.Key.Equals(key))
                {
                    if (action.ExecuteAsynchronously)
                    {
                        action?.InvokeAsync();
                        return;
                    }
                    action?.Invoke();
                }
        }
        private static void Input_OnKeyDown(Key key)
        {
            foreach (InputAction action in InputActions_KeyDown)
                if (action.Key.Equals(key))
                {
                    if (action.ExecuteAsynchronously)
                    {
                        action?.InvokeAsync();
                        return;
                    }
                    action?.Invoke();
                }
        }
    }

    public class InputAction
    {
        internal Key Key; 
        internal readonly bool ExecuteAsynchronously = false;
        private ValueTuple<Action<object[]?>, object[]?> Action_Args = new();

        public InputAction(bool async, Action<object[]?> expression, object[] args, Key key)
        {
            ExecuteAsynchronously = async;
            Action_Args.Item1 = expression;
            Action_Args.Item2 = args;
            Key = key; 
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