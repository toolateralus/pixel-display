﻿using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Linq;

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
                EditorEvent(e);
            }
        }
        protected internal void EditorEvent(EditorEvent e)
        {
            e.action?.Invoke(e.args);
            if (e.message is "" || e.message.Contains("$nolog"))
            {
                if (e is FocusNodeEvent nodeEvent && nodeEvent.args.First() is Node node)
                {
                    Editor.Current.ActivelySelected.Add(node);
                    Editor.Current.LastSelected = node;
                    Editor.Current.Inspector.DeselectNode();
                    Editor.Current.Inspector.SelectNode(node);
                    StageCameraTool.TryFollowNode(node);
                }
                return;
            }

            if (Editor.Current.consoleOutput.Items.Count >= Editor.Current.settings.ConsoleMaxLines)
                Console.Clear();

            Editor.Current.consoleOutput.Items.Add(e.message);
            Editor.Current.consoleOutput.ScrollIntoView(e.message);
        }
    }
}

