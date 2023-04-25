using System;
namespace pixel_renderer
{
    [Flags]
    public enum EditorEventFlags
    {
        PRINT = 0,
        PRINT_ERR = 1,
        DO_NOT_PRINT = 2,
        COMPONENT_EDITOR_UPDATE = 4,
        FILE_VIEWER_UPDATE = 8,
        GET_FILE_VIEWER_SELECTED_METADATA = 16,
        GET_FILE_VIEWER_SELECTED_OBJECT = 32,
        FOCUS_NODE = 64,
        CLEAR_CONSOLE = 128,
    }
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
        public EditorEventFlags flags =  EditorEventFlags.GET_FILE_VIEWER_SELECTED_OBJECT; 

        public EditorEvent(EditorEventFlags flags, string message = "", bool includeDateTime = false, bool clearConsole = false)
        {
            this.flags = flags;
            ClearConsole = clearConsole;
            if (!includeDateTime)
                this.message = message; 
            else
                this.message = DateAndTime + message;
        }
    }
}