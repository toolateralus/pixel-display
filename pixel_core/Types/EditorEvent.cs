using System;

namespace Pixel
{
    /// <summary>
    /// Shortcut flags for EditorEvents to quickly make requests/calls.
    /// </summary>
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
        GET_COMMAND_LIBRARY_C_SHARP = 256,
    }
    /// <summary>
    /// This is a class used to pass info and requests to the Editor without the context neccesary from being lower in the hierarchy.
    /// </summary>
    /// 
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

        public bool processed = false;

        public EditorEventFlags flags = EditorEventFlags.GET_FILE_VIEWER_SELECTED_OBJECT;

        public EditorEvent(EditorEventFlags flags, string message = "", bool includeDateTime = false)
        {
            this.flags = flags;
            if (!includeDateTime)
                this.message = message;
            else
                this.message = DateAndTime + message;
        }
    }
}