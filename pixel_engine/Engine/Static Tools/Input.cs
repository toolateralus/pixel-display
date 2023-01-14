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
                var isDown = Keyboard.IsKeyDown(key);
                if (isDown)
                    if (action.ExecuteAsynchronously)
                    {
                        Task.Run(() => action.InvokeAsync());
                        return;
                    }
                    else action.Invoke();
            }
        }
        public static void RegisterAction(bool async, Action<object[]?> action, object[]? args, Key key, InputEventType type) => InputActions.Add(new(action, key));
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