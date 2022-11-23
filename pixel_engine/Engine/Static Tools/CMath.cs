namespace pixel_renderer
{
    using System;

    public static class CMath
    {
        public const int Gravity = 1;
        public const float PI = MathF.PI;
        public const float Tau = MathF.PI * 2;
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        public static float Power(float value, int power)
        {
            float output = 1f;
            for (int i = 0; i < power; i++)
            {
                output *= value;
            }
            return output;
        }
        internal static Vec2 Negate(Vec2 v) => new()
        {
            x = -v.x,
            y = -v.y,
        };

        internal static Vec3 Negate(Vec3 v) => new()
        {
            x = -v.x,
            y = -v.y,
            z = -v.z
        };

    }

}


