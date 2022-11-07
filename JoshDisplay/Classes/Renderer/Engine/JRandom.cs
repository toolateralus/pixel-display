namespace pixel_renderer
{
    using Random = System.Random;
    using Color = System.Drawing.Color;

    public static class JRandom
    {
        public static Vec2 ScreenPosition()
        {
            var pos = new Vec2();

            var xLimit = Constants.screenHeight;
            var yLimit = Constants.screenWidth;

            return new()
            {
                x = Int(0, xLimit),
                y = Int(0, yLimit)
            };
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
        public static int Int(int min, int max) => new Random().Next(min, max);
        public static bool Bool() => Int(-32000, 32000) > 0;
        public static bool Bool(int odds) => Int(-odds, odds) > 0;
    }

}


