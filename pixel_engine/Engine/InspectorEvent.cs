using System;

namespace pixel_renderer
{
    public abstract class InspectorEvent
    {
        public string message;
        public object sender;
        public Action<object?, object?, object?, object?> expression = (object? arg1, object? arg2, object? arg3, object? arg4) => { };
        public object[] expressionArgs = new object[3];
    }

}