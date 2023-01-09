using pixel_renderer;
using pixel_editor;
using System;
using System.Threading.Tasks;
namespace pixel_editor
{
    public static class Console
    {
        public static void Print(object? o)
        {
            var msg = o.ToString();
            var e = EditorMessage.New(msg);
            Runtime.RaiseInspectorEvent(e);
        }
        public static void Error(object? o = null, int? delay = null)
        {
            string? msg = o.ToString();
            EditorMessage e = EditorMessage.New(msg);
            var inspector = (Runtime.inspector as Inspector);

            if (inspector is not null)
                if (delay is not null)
                {
                    Action<object[]?> c = RedTextForMS(inspector, (int)delay);
                    e.expression = c;
                }
            Runtime.RaiseInspectorEvent(e);
        }
        public static Action<object[]?> RedTextForMS(Inspector? inspector, int delay)
        {
            Action<object?> c = async (o) =>
            {
                Editor.Current.RedText(null);
                await Task.Delay(delay);
                Editor.Current.BlackText(null);
            };
            return c;
        }
    }
}
