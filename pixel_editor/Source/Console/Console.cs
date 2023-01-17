using pixel_renderer;
using pixel_editor;
using System;
using System.Threading.Tasks;
namespace pixel_editor
{
    public static class Console
    {
        public static void Print(object? o, bool includeDateTime = false)
        {
            var msg = o.ToString();
            var e = new EditorEvent(msg, includeDateTime);
            Editor.QueueEvent(e); 
        }
        public static void Error(object? o = null, int? textColorAlterationDuration = null)
        {
            string? msg = o.ToString();
            EditorEvent e = new(msg, true);
           
                if (textColorAlterationDuration is not null)
                    e.action = RedTextForMsAsync( (int)textColorAlterationDuration);
            Editor.QueueEvent(e);
        }

        public static Action<object[]?> RedTextForMsAsync(int delay)
        {
            return async (o) =>
            {
                Editor.Current.RedText().Invoke(o);
                await Task.Delay(delay * 1000);
                Editor.Current.BlackText().Invoke(o);
            };
        }
        public static void Clear()
        {
            EditorEvent editorEvent = new EditorEvent("");
            editorEvent.ClearConsole = true;
            Editor.QueueEvent(editorEvent);

            Error("Console Cleared", 1);
        }
    }
}
