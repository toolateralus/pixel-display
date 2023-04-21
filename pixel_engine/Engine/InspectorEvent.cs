using System;
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
        public bool processed = false;

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
    public class FocusNodeEvent : EditorEvent
    {
        public FocusNodeEvent(Node focused, string message = "$nolog", bool includeDateTime = false, bool clearConsole = false) : base(message, includeDateTime, clearConsole)
        {
            action = (e) => { };
            args = new object[] { focused };
        }
    }
}