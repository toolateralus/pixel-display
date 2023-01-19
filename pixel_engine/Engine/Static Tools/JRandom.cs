﻿namespace pixel_renderer
{
    using System.Security.Cryptography;
    using Color = System.Drawing.Color;
    using Random = System.Random;

    public static class JRandom
    {

        public static Direction Direction() => (Direction)Int(0, sizeof(Direction) - 1);


        public static Color Color()
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
            if (min > max)
            {
                Runtime.Log("Random int min was greater than the max");
                return min;
            }
            return min + RandomNumberGenerator.GetInt32(max - min);
        }

        public static bool Bool() => Int(-32000, 32000) > 0;
        public static bool Bool(int odds) => Int(-odds, odds) > 0;

        internal static Vec2 Vec2(Vec2 min, Vec2 max)
        {
            return new()
            {
                x = Int((int)min.x, (int)max.x),
                y = Int((int)min.y, (int)max.y),
            };
        }
    }

}


