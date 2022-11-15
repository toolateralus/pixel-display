namespace pixel_renderer
{
    using System.Collections.Generic;
    using System.Windows.Input;

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
                   KeyDown.Add(key, result);

            else KeyDown[key] = result;
        }
        public static void Refresh() 
        {
            foreach (Key key in keys)
                SetKey(key, Keyboard.IsKeyDown(key));
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


