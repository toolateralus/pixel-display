using Pixel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel_Editor
{
    public class EditorEventHandler
    {
        protected internal static Action<EditorEvent> InspectorEventRaised;
        protected internal static Queue<EditorEvent> Pending = new();
        public static void QueueEvent(EditorEvent e)
        {
            if (e.flags.HasFlag(EditorEventFlags.CLEAR_CONSOLE))
                ConsoleWindow.ClearAll();
            Pending.Enqueue(e);
        }
        protected internal void ExecuteAll()
        {
            EditorEvent e;
            for (int i = 0; Pending.Count > 0; ++i)
            {
                e = Pending.Dequeue();
                if (e is null)
                    return;
                ExecuteEditorEvent(e);
            }
        }
        protected internal void ExecuteEditorEvent(EditorEvent e)
        {
            switch (e.flags)
            {
                case EditorEventFlags.PRINT:    
                    break;
                case EditorEventFlags.PRINT_ERR:
                    Console.Error(e.message, 1);
                    break;

                case EditorEventFlags.DO_NOT_PRINT:
                case EditorEventFlags.COMPONENT_EDITOR_UPDATE:
                    var component = Editor.Current.componentEditor.component;
                    Editor.Current?.componentEditor.Refresh(component);
                    break;

                case EditorEventFlags.FILE_VIEWER_UPDATE:
                    Editor.Current.fileViewer.Refresh();
                    break;

                case EditorEventFlags.GET_FILE_VIEWER_SELECTED_METADATA:
                    var obj = Editor.Current.fileViewer.GetSelectedMeta();
                    if (obj != null)
                        e.args = new object[] { obj };
                    e.action.Invoke(e.args);
                    return;

                case EditorEventFlags.GET_COMMAND_LIBRARY_C_SHARP:
                    var cmds = Console.Current.LoadedCommands.ToArray();
                    e.args = new object[] { cmds };
                    e.action.Invoke(e.args);
                    return;
                case EditorEventFlags.GET_FILE_VIEWER_SELECTED_OBJECT:
                    var obj1 = Editor.Current.fileViewer.GetSelectedObject();
                    if (obj1 != null)
                        e.args = new object[] { obj1 };
                    e.action.Invoke(e.args);
                    return;
                case EditorEventFlags.FOCUS_NODE:
                    if (e.args.Any() && e.args.First() is Node node)
                    {
                        Editor.Current.ActivelySelected.Add(node);
                        Editor.Current.LastSelected = node;
                        Editor.Current.Inspector.DeselectNode();
                        Editor.Current.Inspector.SelectNode(node);
                        StageCameraTool.TryFollowNode(node);
                    }
                    break;

            }

            e.action?.Invoke(e.args);
            e.processed = true;;

            ConsoleWindow.SendMessage(e.message);
        }
    }
}

