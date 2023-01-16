using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

namespace pixel_renderer
{

    public enum InputEventType { KeyDown, KeyUp, KeyToggle }
    public static class Input
    {
        static List<InputAction> InputActions = new(250);
        internal static void Awake()
        {
          
        }
        internal static void Refresh()
        {
            foreach (var action in InputActions)
            {
                var type = action.EventType;
                var key = action.Key;
                bool input_value;
                input_value = GetInputValueByType(type, key);
                if (input_value)
                    if (action.ExecuteAsynchronously)
                    {
                        Task.Run(() => action.InvokeAsync());
                        return;
                    }
                    else action.Invoke();
            }
        }
        public static bool GetInputValueByType_Name(InputEventType type, string key)
        {
            Key key_ = Enum.Parse<Key>(key);
            var input_value = type switch
            {
                InputEventType.KeyDown => Keyboard.IsKeyDown(key_),
                InputEventType.KeyUp => Keyboard.IsKeyUp(key_),
                InputEventType.KeyToggle => Keyboard.IsKeyToggled(key_),
                _ => false,
            };
            return input_value;
        }
        public static bool GetInputValueByType(InputEventType type, Key key)
        {
            
            var input_value = type switch
            {
                InputEventType.KeyDown => Keyboard.IsKeyDown(key),
                InputEventType.KeyUp => Keyboard.IsKeyUp(key),
                InputEventType.KeyToggle => Keyboard.IsKeyToggled(key),
                _ => false,
            };
            return input_value;
        }
        public static void RegisterAction(Action<object[]?> action, Key key , object[]? args = null, bool async = false, InputEventType type = InputEventType.KeyDown) => InputActions.Add(new(action, key));
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