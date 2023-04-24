﻿using pixel_renderer;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace pixel_editor
{
    public class EditorEventHandler
    {
        protected internal static Editor Editor => Editor.Current;
        protected internal Action<EditorEvent> InspectorEventRaised;
        protected internal Stack<EditorEvent> Pending = new();
        protected internal void ExecuteAll()
        {
            EditorEvent e;
            for (int i = 0; Pending.Count > 0; ++i)
            {
                e = Pending.Pop();
                if (e is null)
                    return;
                ExecuteEditorEvent(e);
            }
        }
        protected internal void ExecuteEditorEvent(EditorEvent e)
        {


            switch (e.flags)
            {
                case EditorEventFlags.DO_NOT_PRINT:
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
                case EditorEventFlags.PRINT_NO_ERROR:
                    
                    break;
                case EditorEventFlags.LOG_ERROR:
                    Console.Error(e.message, 1);
                    return;
                case EditorEventFlags.COMPONENT_EDITOR_UPDATE:
                    var component = Editor.Current.componentEditor.component;
                    Editor.Current?.componentEditor.Refresh(component);
                    break;
                case EditorEventFlags.FILE_VIEWER_UPDATE:
                    Editor.Current.fileViewer.Refresh();
                    break;
                case EditorEventFlags.GET_FILE_VIEWER_SELECTED_METADATA:
                    var obj = Editor.fileViewer.GetSelectedMeta();

                    if (obj != null)
                        e.args = new object[] { obj };
                    e.action?.Invoke(e.args);
                    return;
                case EditorEventFlags.GET_FILE_VIEWER_SELECTED_OBJECT:
                    var obj1 = Editor.fileViewer.GetSelectedObject();

                    if (obj1 != null)
                        e.args = new object[] { obj1 };
                    e.action?.Invoke(e.args);
                    return;
            }

            //// old command system, still exists in paralell but use flags instead.
            //if (e.message is "" || e.message.Contains(EditorEvent.DO_NOT_PRINT))
            //{
            //    if (e is FocusNodeEvent nodeEvent && nodeEvent.args.First() is Node node)
            //    {
            //        Editor.Current.ActivelySelected.Add(node);
            //        Editor.Current.LastSelected = node;
            //        Editor.Current.Inspector.DeselectNode();
            //        Editor.Current.Inspector.SelectNode(node);
            //        StageCameraTool.TryFollowNode(node);
            //    }
            //    if (e.message == EditorEvent.LOG_ERROR || e.message.Contains(EditorEvent.LOG_ERROR))
            //    {
            //        var msg = e.message.Replace(EditorEvent.LOG_ERROR, "");
            //        var newMsg = msg.Replace(EditorEvent.DO_NOT_PRINT, "");
            //        Console.Error(newMsg, 1);
            //        return; 
            //    }
            //    if (e.message == EditorEvent.GET_FILE_VIEWER_SELECTED_OBJECT)
            //    {
            //        var obj = Editor.fileViewer.GetSelectedObject();
                    
            //        if(obj != null)
            //            e.args = new object[] { obj };
            //        e.action?.Invoke(e.args);
            //        return; 
            //    }
            //    if (e.message == EditorEvent.GET_FILE_VIEWER_SELECTED_METADATA)
            //    {
            //        var obj = Editor.fileViewer.GetSelectedMeta();

            //        if (obj != null)
            //            e.args = new object[] { obj };
            //        e.action?.Invoke(e.args);
            //        return;
            //    }
              
            //    if (e.message == EditorEvent.COMPONENT_EDITOR_UPDATE)
            //    {
            //        var component = Editor.Current.componentEditor.component;
            //        Editor.Current?.componentEditor.Refresh(component);
            //    }
                
            //    if (e.message == EditorEvent.UPDATE_FILEVIEWER)
            //    {
            //        Editor.Current.fileViewer.Refresh();
            //    }


            //    return;
            //}
            
            e.action?.Invoke(e.args);
            e.processed = true;

            if (Editor.Current.consoleOutput.Items.Count >= Editor.Current.settings.ConsoleMaxLines)
                Console.Clear();

            Editor.Current.consoleOutput.Items.Add(e.message);
            Editor.Current.consoleOutput.ScrollIntoView(e.message);
        }
    }
}

