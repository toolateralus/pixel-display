
namespace pixel_renderer
{
    using System;
    using System.Runtime.CompilerServices;

    public static class ExtensionMethods
    {
        public static bool WithinRange(this float v, float min, float max) { return v <= max && v >= min; }
        public static Vec2 WithValue(this Vec2 v, float? x = null, float? y = null) { return new Vec2(x ?? v.x, y ?? v.x); }
        public static Vec2 WithScale(this Vec2 v, float x = 1, float y = 1) { return new Vec2(v.x * x, v.y * y); }
        public static double Sum(this Vec2 v) => v.x + v.y;
        public static double Sum(this Vec3 v) => v.x + v.y + v.z;
        public static float Distance(this Vec2 v, Vec2 end) => (v - end).Length;
        /// <summary>
        /// fix this divide by zero
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vec2 Normalize(this Vec2 v) => v / v.Length;
        public static double Clamp(this double v, double min, double max)
        {
            return Math.Min(max, Math.Max(v, min));
        }

    }
}
