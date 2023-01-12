using System;
using System.Diagnostics.CodeAnalysis;

namespace pixel_renderer
{
    public class InspectorEvent
    {
        public string message;
        public object? sender;
        public Action<object[]?> action = (e) => { };
        public object[]? args = new object[3];
        public InspectorEvent(string message) => this.message = DateTime.Now.ToLocalTime().ToLongDateString() + " " +DateTime.Now.ToLocalTime().ToLongTimeString() + "\n " + message;
        public InspectorEvent(string message, object? sender, Action<object[]?> action, object[]? args) : this(message)
        {
            this.action = action;
            this.args = args;
            this.sender = sender;
        }
   
    }
}