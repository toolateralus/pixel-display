namespace pixel_renderer
{
    using Color = System.Drawing.Color;
    using Random = System.Random;

    public static class JRandom
    {

        public static Direction Direction() => (Direction)Int(0, sizeof(Direction) - 1);

        public static Vec2 ScreenPosition() => new()
        {
            x = Int(0, Settings.ScreenWidth),
            y = Int(0, Settings.ScreenHeight)
        };

        public static Color Color()
        {
            byte r = Byte(),
                 g = Byte(),
                 b = Byte(),
                 a = Byte();

            return System.Drawing.Color.FromArgb(r, g, b, a);
        }
        public static byte Byte() => (byte)Int(0, 255);
        public static int Int(int min, int max) => new Random().Next(min, max);
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


