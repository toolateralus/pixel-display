using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pixel_core.types.physics;

namespace pixel_core
{
    internal class JsonListBuilder
    {
        public List<object> objects = new();
        public string JsonList
        {
            get
            {
                StringBuilder output = new();
                output.Append('[');
                for (int i = 0; i < objects.Count; i++)
                {
                    output.Append(IO.WriteJsonToString(objects[i]));
                    if (i < objects.Count - 1)
                        output.Append(',');
                }
                output.Append(']');
                return output.ToString();
            }
        }
        public JsonListBuilder() { }
        public JsonListBuilder(Polygon A, Polygon B)
        {
            A.debuggingColor = System.Drawing.Color.FromArgb(50, 255, 0, 0);
            B.debuggingColor = System.Drawing.Color.FromArgb(50, 0, 0, 255);
            objects.Add(A);
            objects.Add(B);
        }
    }
}
