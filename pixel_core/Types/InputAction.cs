using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pixel.Types
{
    public enum InputEventType { KeyDown, KeyUp, KeyToggle }
    public class InputAction
    {
        public Key Key;
        public InputEventType EventType = InputEventType.KeyDown;
        public readonly bool ExecuteAsynchronously = false;
        public Action action;
        public InputAction(Action action, Key key, object[]? args = null, bool async = false, InputEventType type = InputEventType.KeyDown)
        {
            this.action = action;
            ExecuteAsynchronously = async;
            Key = key;
            EventType = type;
        }
        public void Invoke() => action?.Invoke();
        public async Task InvokeAsync(float? delay = null)
        {
            if (delay is not null)
                await Task.Delay((int)delay);
            await Task.Run(() => action?.Invoke());
        }
    }
}