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
        public InspectorEvent(string message, bool? includeDateTime = false)
        {
            if (includeDateTime is null or false)
                this.message = message; 
            else this.message = DateAndTime + message;
        }
        public string DateAndTime => DateTime.Now.ToLocalTime().ToLongDateString()
            + " "
            + DateTime.Now.ToLocalTime().ToLongTimeString()
            + "\n"; 
        public InspectorEvent(string message, object? sender, Action<object[]?> action, object[]? args) : this(message)
        {
            this.action = action;
            this.args = args;
            this.sender = sender;
        }
   
    }
}