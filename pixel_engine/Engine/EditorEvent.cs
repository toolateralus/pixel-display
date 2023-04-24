﻿using System;
namespace pixel_renderer
{
    [Flags]
    public enum EditorEventFlags
    {
        PRINT_NO_ERROR = 0,
        LOG_ERROR = 1,
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
    public class FocusNodeEvent : EditorEvent
    {
        public FocusNodeEvent(Node focused, EditorEventFlags flags = EditorEventFlags.FOCUS_NODE | EditorEventFlags.DO_NOT_PRINT, string message = "", bool includeDateTime = false, bool clearConsole = false) : base(flags, message, includeDateTime, clearConsole)
        {
            action = (e) => { };
            args = new object[] { focused };
        }
    }
}