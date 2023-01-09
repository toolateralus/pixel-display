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
        public static void Error(object? o)
        {
            var msg = o.ToString();
            var e = EditorMessage.New(msg);
            var inspector = (Runtime.inspector as Inspector);

            if (inspector is not null)
            {
                Action<object?> a = inspector.RedText(null);
                Action<object?> b = inspector.BlackText(null);
                Action<object?> c = async (o) =>
                {
                   a.Invoke(null);  
                   await Task.Delay(500);
                   b.Invoke(null);
                };
                e.expression = c; 
            }

            Runtime.RaiseInspectorEvent(e);
        }

    }
}
