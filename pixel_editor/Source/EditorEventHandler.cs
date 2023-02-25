using pixel_renderer;
using System;
using System.Collections.Generic;

namespace pixel_editor
{
    public class EditorEventHandler
    {
        protected internal static Editor Editor => Editor.Current;
        protected internal Action<EditorEvent> InspectorEventRaised;
        protected internal Queue<EditorEvent> Pending = new();
        protected internal void ExecuteAll()
        {
            EditorEvent e;
            for (int i = 0; Pending.Count > 0; ++i)
            {
                e = Pending.Dequeue();
                if (e is null)
                    return;
                Editor.Current.EditorEvent(e);
            }
        }
    }
}

