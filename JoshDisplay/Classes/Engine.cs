using System.Collections.Generic;
using System.Windows.Input;

namespace PixelRenderer
{
    namespace MathP { }

    public class Vec2
    {
        public float x; public float y;
        
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
