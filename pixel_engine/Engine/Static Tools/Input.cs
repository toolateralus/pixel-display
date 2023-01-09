namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using static System.Runtime.CompilerServices.RuntimeHelpers;

    public static class Input
    {
        static Dictionary<Key, bool> KeyDown = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeyUp = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeyToggled = new Dictionary<Key, bool>();

        static Key[] keys =
        {
            Key.W, 
            Key.S,
            Key.A,
            Key.D,
            
            Key.Space,
            Key.Q,
            Key.F1,
            Key.F2, 
            Key.F3,
            Key.F4,
            Key.F5
        };
        static Dictionary<string, Key> s_keys = new()
        {
            { "up", Key.W },
            { "down", Key.S },
            { "left", Key.A },
            { "right", Key.D },
        };
        
        public static void Refresh()
        {
            foreach (Key key in keys)
                SetKeyDown(key, Keyboard.IsKeyDown(key));
            foreach (Key key in keys)
                SetKeyUp(key, Keyboard.IsKeyUp(key));
            foreach (Key key in keys)
                SetKeyToggled(key, Keyboard.IsKeyToggled(key));
        }
        
        public static bool GetKeyDown(Key key)
        {
            return KeyDown.ContainsKey(key) && KeyDown[key];
        }
        public static bool GetKeyUp(Key key)
        {
            return KeyUp.ContainsKey(key) && KeyUp[key];
        }
        public static bool GetKeyToggled(Key key)
        {
            return KeyToggled.ContainsKey(key) && KeyToggled[key];
        }

        public static bool GetKeyDown(string keycode)
        {
            if (!s_keys.ContainsKey(keycode)) return false;
            Key key = s_keys[keycode];
            return KeyDown.ContainsKey(key) && KeyDown[key];
        }
        public static bool GetKeyUp(string keycode)
        {
            if (!s_keys.ContainsKey(keycode)) return false;
            Key key = s_keys[keycode];
            return KeyUp.ContainsKey(key) && KeyUp[key];
        }
        public static bool GetKeyToggled(string keycode)
        {
            if (!s_keys.ContainsKey(keycode)) return false;
            Key key = s_keys[keycode];
            return KeyToggled.ContainsKey(key) && KeyToggled[key];
        }

        private static void SetKeyDown(Key key, bool result)
        {
            if (!KeyDown.ContainsKey(key))
                KeyDown.Add(key, result);
            else KeyDown[key] = result;
        }
        private static void SetKeyToggled(Key key, bool result)
        {
            if (!KeyToggled.ContainsKey(key))
                KeyToggled.Add(key, result);
            else KeyToggled[key] = result;
        }
        private static void SetKeyUp(Key key, bool result)
        {
            if (!KeyUp.ContainsKey(key))
                KeyUp.Add(key, result);
            else KeyUp[key] = result;
        }
        
        /// <summary>
        /// Get a Vec2 representing WASD input values, with a vector max range of -1 to 1 
        /// </summary>
        /// <returns>new Vec2(right - left, up - down) between -1 and 1</returns>
        public static Vec2 GetMoveVector()
        {
            bool A = GetKeyDown(Key.A);
            bool D = GetKeyDown(Key.D);
            bool W = GetKeyDown(Key.W);
            bool S = GetKeyDown(Key.S);

            int down = W ? 0 : 1;
            int up = S ? 0 : 1;
            int right = D ? 0 : 1;
            int left = A ? 0 : 1;

            return new Vec2(left - right, down - up);
        }
    }

}


