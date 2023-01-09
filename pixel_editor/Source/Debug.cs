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
            var msg = o.ToString();
            var e = EditorMessage.New(msg);
            var inspector = (Runtime.inspector as Inspector);

            if (inspector is not null)
            {
                if (delay is not null)
                {
                    Action<object[]?> c = RedTextForSeconds(inspector, (int)delay);
                    e.expression = c;
                }
            }
            Runtime.RaiseInspectorEvent(e);
        }
        private static Action<object[]?> RedTextForSeconds(Inspector? inspector, int delay)
        {
            Action<object?> a = inspector.RedText(null);
            Action<object?> b = inspector.BlackText(null);
            Action<object?> c = async (o) =>
            {
                a.Invoke(null);
                await Task.Delay(delay);
                b.Invoke(null);
            };
            return c;
        }
    }
}
