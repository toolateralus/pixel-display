using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_editor
{
    public class Console
    {
        public static void Print(object? o)
        {
            var msg = o.ToString();
            var e = EditorMessage.New(msg);
            Runtime.RaiseInspectorEvent(e);
        }
        public static void Error(object? o = null, float? delay = null)
        {
            var msg = o.ToString();
            var e = EditorMessage.New(msg);
            var inspector = (Runtime.inspector as Inspector);

            if (inspector is not null)
            {
                if (delay is not null and not 0 and < int.MaxValue and > int.MinValue)
                {
                    Action<object[]?> c = RedTextForSeconds(inspector, (int)delay);
                    e.expression = c;
                }
                else
                {
                    Action<object?> a = inspector.RedText(null);
                    Action<object?> b = inspector.BlackText(null);
                    Action<object?> c = (o) =>
                    {
                        a.Invoke(null);
                        b.Invoke(null);
                    };
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
                Print($"A Invoked {DateTime.Now.ToLocalTime()}");
                await Task.Delay(delay);
                Print($"B Invoked after Wait {DateTime.Now.ToLocalTime()}");
                b.Invoke(null);
            };
            return c;
        }
    }
}
