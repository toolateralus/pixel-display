namespace pixel_renderer
{
    using System.Security.Cryptography;
    using Color = System.Drawing.Color;
    using Random = System.Random;

    public static class JRandom
    {

        public static Direction Direction() => (Direction)Int(0, sizeof(Direction) - 1);

        public static Color Color(byte aMin = 0, byte rMin = 0, byte gMin = 0, byte bMin = 0, byte aMax = 255, byte rMax = 255, byte gMax = 255, byte bMax = 255)
        {
            var A = Int(aMin, aMax);
            var R = Int(rMin, rMax);
            var G = Int(gMin, gMax);
            var B = Int(bMin, bMax);
            return System.Drawing.Color.FromArgb(A, R, G, B);
        }
        public static Color OpaqueColor()
        {
            return Color(255);
        }

      
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
            if (min >= max)
            {
                return min;
            }
            return min + RandomNumberGenerator.GetInt32(max - min);
        }

        public static bool Bool() => Int(-32000, 32000) > 0;

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


