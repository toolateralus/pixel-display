using System;
using System.Diagnostics.CodeAnalysis;

namespace pixel_renderer
{
    public class EditorEvent
    {
        public string message;
        public object? sender;
        public Action<object[]?> action = (e) => { };
        public object[]? args = new object[3];
        public string DateAndTime => DateTime.Now.ToLocalTime().ToLongDateString()
            + " "
            + DateTime.Now.ToLocalTime().ToLongTimeString()
            + "\n"; 
        public bool ClearConsole = false; 

        public EditorEvent(string message, bool includeDateTime = false, bool clearConsole = false)
        {
            ClearConsole = clearConsole;
            if (!includeDateTime)
                this.message = message; 
            else
            {
                this.message = DateAndTime + message;
            }
        }
    }
}