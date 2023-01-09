using System;

namespace pixel_renderer
{
    public abstract class InspectorEvent
    {
        public string message;
        public object? sender;
        public Action<object[]?>? expression = (e) => { };
        public object[]? args = new object[3];
    }
}