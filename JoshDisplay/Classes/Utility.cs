namespace PixelRenderer
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Color = System.Drawing.Color;
    public static class Constants
    {
        public const int frameRateCheckThresh = 60;
        public const int screenWidth = 64;
        public const int screenHeight = 64;
    }
    public static class CMath
    {
        public const float Gravity = 1f; 
        public const float PI = MathF.PI; 
        public const float Tau = MathF.PI * 2;
        public static float Power(float value, int power)
        {
            float output = 1f; 
            for (int i = 0; i < power; i++)
            {
                output *= value; 
            }
            return output; 
        }
    }
    public static class JRandom
    {
        public static Color GetRandomColor()
        {
            byte r = RandomByte(),
                g = RandomByte(),
                b = RandomByte(),
                a = RandomByte();

            return Color.FromArgb(r, g, b, a);
        }
        public static byte RandomByte() => (byte)Random.Shared.Next(0, 255);
        public static int RandomInt(int min, int max) => Random.Shared.Next(min, max);
      
           

    }
    public class Vec2
    {
        public float x;
        public float y;
        public float Length => x * x + y * y;
        public static Vec2 one = new Vec2(1, 1);
        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public Vec2()
        { }
        public static Vec2 operator +(Vec2 a, Vec2 b) { return new Vec2(a.x + b.x, a.y + b.y); }
        public static Vec2 operator -(Vec2 a, Vec2 b) { return new Vec2(a.x - b.x, a.y - b.y); }
        public static Vec2 operator /(Vec2 a, float b) { return new Vec2(a.x / b, a.y / b); }
        public static Vec2 operator *(Vec2 a, float b) { return new Vec2(a.x * b, a.y * b); }
    }
    public static class Input
    {
        static Dictionary<Key, bool> KeyDown = new Dictionary<Key, bool>();
        static Key[] keys =
        {
            Key.W, Key.A, Key.S, Key.D, Key.Space, Key.Q
        };
        public static bool GetKeyDown(Key key)
        {
            if (KeyDown.ContainsKey(key))
            {
                return KeyDown[key];
            }
            return false;
        }
        public static void SetKey(Key key, bool result)
        {
            if (!KeyDown.ContainsKey(key))
            {
                KeyDown.Add(key, result);
            }
            else KeyDown[key] = result;
        }
        public static void UpdateKeyboardState()
        {
            foreach (Key key in keys)
            {
                SetKey(key, Keyboard.IsKeyDown(key));
            }
        }
    }
}
