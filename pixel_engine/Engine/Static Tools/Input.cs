using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Windows;

namespace pixel_renderer
{

    public enum InputEventType { Down, Up, Toggle }
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
                    bool input_value = GetInputValueByType(type, ref action.Key);
                    if (input_value)
                        if (action.ExecuteAsynchronously) Task.Run(() => action.InvokeAsync());
                        else action.Invoke();
                }
            }
        }
        public static bool GetInputValue(InputEventType type, string key)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                Key key_ = Enum.Parse<Key>(key);
                var input_value = type switch
                {
                    InputEventType.Down => Keyboard.IsKeyDown(key_),
                    InputEventType.Up => Keyboard.IsKeyUp(key_),
                    InputEventType.Toggle => Keyboard.IsKeyToggled(key_),
                    _ => false,
                };
                return input_value;
            });
        }
        public static bool GetInputValueByType(InputEventType type, ref Key key)
        {
            Key _key = key; 
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var input_value = type switch
                {
                    InputEventType.Down => Keyboard.IsKeyDown(_key),
                    InputEventType.Up => Keyboard.IsKeyUp(_key),
                    InputEventType.Toggle => Keyboard.IsKeyToggled(_key),
                    _ => false,
                };
                    return input_value;
            });
        }
        public static void RegisterAction(Action<object[]?> action, Key key, object[]? args = null, bool async = false, InputEventType type = InputEventType.Down)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (InputActions)
                    InputActions.Add(new(action, key));
            });
        }
    }

    public class InputAction
    {
        internal Key Key;
        internal InputEventType EventType = InputEventType.Down; 
        internal readonly bool ExecuteAsynchronously = false;
        private ValueTuple<Action<object[]?>, object[]?> Action_Args = new();

        public InputAction(Action<object[]?> expression, Key key, object[]? args = null, bool async = false, InputEventType type = InputEventType.Down)
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