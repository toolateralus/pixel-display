using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pixel_renderer
{
    public class InputAction
    {
        internal Key Key;
        internal InputEventType EventType = InputEventType.KeyDown; 
        internal readonly bool ExecuteAsynchronously = false;
        internal Action action;
        public InputAction(Action expression, Key key, object[]? args = null, bool async = false, InputEventType type = InputEventType.KeyDown)
        {
            ExecuteAsynchronously = async;
            Key = key;
            EventType = type;
        }
        internal void Invoke() => action?.Invoke();
        internal async Task InvokeAsync(float? delay = null)
        {
            if (delay is not null)
                await Task.Delay((int)delay);
            await Task.Run(() => action?.Invoke());
        }
    }
}