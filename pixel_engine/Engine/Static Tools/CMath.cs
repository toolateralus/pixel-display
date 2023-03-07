namespace pixel_renderer
{
    using System;
    using System.Numerics;

    public static class CMath
    {
        public static Vector2 Gravity = new(0,0.02f);
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
                output *= value;
            return output;
        }
        public static double Negate(double v) => -v;
        public static Vector2 Negate(Vector2 v) => new()
        {
            X = -v.X,
            Y = -v.Y,
        };

        public static Vector3 Negate(Vector3 v) => new()
        {
            X = -v.X,
            Y = -v.Y,
            Z = -v.Z
        };
        public static Vector2 Min(Vector2 v1, Vector2 v2)
        {
            return new(MathF.Min(v1.X ,v2.X), MathF.Min(v1.Y, v2.Y));
        }
        public static float Sqrt(float input)
        {
            return MathF.Sqrt(input);
        }
    }

}


