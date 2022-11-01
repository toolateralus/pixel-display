using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRenderer 
{
    public static class ExtensionMethods
    {
        public static bool WithinRange(this float v, float min, float max) { return v <= max && v >= min; }
        public static Vec2 WithValue(this Vec2 v, float? x = null, float? y = null) { return new Vec2(x ?? v.x, y ?? v.x); }
        public static Vec2 WithScale(this Vec2 v, float x = 1, float y = 1) { return new Vec2(v.x * x, v.y * y); }
        public static double Clamp(this double self, double min, double max)
        {
            return Math.Min(max, Math.Max(self, min));
        }
     
    }
}
