namespace pixel_core
{
    using System;
    using System.Numerics;
    using System.Security.Cryptography;
    using Random = System.Random;
    public enum Direction
    { Left, Right, Up, Down };
    public static class JRandom
    {
        public static Random random = new();
        public static Direction Direction() => (Direction)Int(0, sizeof(Direction) - 1);

        public static Pixel Color(byte aMin = 0, byte rMin = 0, byte gMin = 0, byte bMin = 0, byte aMax = 255, byte rMax = 255, byte gMax = 255, byte bMax = 255)
        {
            var A = Int(aMin, aMax);
            var R = Int(rMin, rMax);
            var G = Int(gMin, gMax);
            var B = Int(bMin, bMax);
            return System.Drawing.Color.FromArgb(A, R, G, B);
        }
        public static Pixel OpaqueColor()
        {
            return Color(255);
        }
        public static Vector2 Velocity(float speed = -1f)
        {
            float x = (float)random.NextDouble() * 2 - 1;
            float y = (float)random.NextDouble() * 2 - 1;
            var dir = new Vector2(x, y).Normalized();

            if (speed == -1f)
                speed = (float)random.NextDouble();

            return (dir * speed);
        }
        public static Pixel Pixel()
        {
            byte r = Byte(),
                 g = Byte(),
                 b = Byte(),
                 a = Byte();

            return System.Drawing.Color.FromArgb(r, g, b, a);
        }
        public static byte Byte() => (byte)Int(0, 255);
        public static int Int(int min, int max)
        {
            if (min >= max)
            {
                return min;
            }
            return min + RandomNumberGenerator.GetInt32(max - min);
        }
        public static bool Bool() => Int(-32000, 32000) > 0;

        internal static Vector2 Vec2(Vector2 min, Vector2 max)
        {
            return new()
            {
                X = Int((int)min.X, (int)max.X),
                Y = Int((int)min.Y, (int)max.Y),
            };
        }
        internal static char Char()
        {
            int v = Int(0, 16);
            if (v >= 10)
                v += 7;
            return Convert.ToChar(v + Convert.ToByte('0'));
        }
        internal static string Hex()
        {
            return RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue).ToString("X");
        }
    }

}


