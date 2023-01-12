using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pixel_renderer
{

    public static class Input
    {
        public enum InputEventType { DOWN, UP, TOGGLE,
            ALL
        }
        static Dictionary<Key, bool> KeyDown = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeyUp = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeyToggled = new Dictionary<Key, bool>();
        
        readonly static Dictionary<string, Key> s_keys = new()
        {
            { "up", Key.W },
            { "down", Key.S },
            { "left", Key.A },
            { "right", Key.D },
        };
        /// 101 keys on US Standard Keyboard.
        static List<InputAction> InputActions_KeyDown = new(101);
        static List<InputAction> InputActions_KeyUp = new(101);
        static List<InputAction> InputActions_KeyToggled = new(101);
        public static event Action<Key>? OnKeyDown;
        public static event Action<Key>? OnKeyUp;
        public static event Action<Key>? OnKeyToggled;
        public static void RegisterAction(InputAction action, InputEventType type)
        {
            switch (type)
            {
                case InputEventType.DOWN:
                    InputActions_KeyDown.Add(action);
                    break;
                case InputEventType.UP:
                    InputActions_KeyUp.Add(action);
                    break;
                case InputEventType.TOGGLE:
                    InputActions_KeyToggled.Add(action);
                    break;
                case InputEventType.ALL:
                    InputActions_KeyToggled.Add(action);
                    InputActions_KeyUp.Add(action);
                    InputActions_KeyDown.Add(action);
                    break; 
            }
        }
        private static void Input_OnKeyToggled(Key key)
        {
            foreach (InputAction action in InputActions_KeyToggled)
                if (action.Key.Equals(key))
                {
                    if (action.ExecuteAsynchronously)
                    {
                        action?.InvokeAsync();
                        return;
                    }
                    action?.Invoke();
                }
        }
        private static void Input_OnKeyUp(Key key)
        {
            foreach (InputAction action in InputActions_KeyUp)
                if (action.Key.Equals(key))
                {
                    if (action.ExecuteAsynchronously)
                    {
                        action?.InvokeAsync();
                        return;
                    }
                    action?.Invoke();
                }
        }
        private static void Input_OnKeyDown(Key key)
        {
            foreach (InputAction action in InputActions_KeyDown)
                if (action.Key.Equals(key))
                {
                    if (action.ExecuteAsynchronously)
                    {
                        action?.InvokeAsync();
                        return;
                    }
                    action?.Invoke();
                }
        }
        public static void Refresh()
        {
            foreach (var input in InputActions_KeyDown)
            {
                Key key = input.Key;
                SetKeyDown(key, Keyboard.IsKeyDown(key));
            }
           

        }

        private static void IterateActions()
        {
           
        }

        public static bool GetKeyDown(Key key) => KeyDown.ContainsKey(key) && KeyDown[key];
        public static bool GetKeyUp(Key key) => KeyUp.ContainsKey(key) && KeyUp[key];
        public static bool GetKeyToggled(Key key) => KeyToggled.ContainsKey(key) && KeyToggled[key];

        public static bool GetKeyDown(string keycode)
        {
            if (!s_keys.ContainsKey(keycode))
                return false;
            Key key = s_keys[keycode];
            var result = KeyDown.ContainsKey(key) && KeyDown[key];
            return result;
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
            if (result)
                OnKeyDown?.Invoke(key);
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
            
            var output = new Vec2(left - right, down - up); 

            return output;  
        }

        internal static void Awake()
        {
            OnKeyDown += Input_OnKeyDown;
            OnKeyUp += Input_OnKeyUp;
            OnKeyToggled += Input_OnKeyToggled;
        }

      
    }

    public class InputAction
    {
        internal Key Key; 
        internal readonly bool ExecuteAsynchronously = false;
        private ValueTuple<Action<object[]?>, object[]?> Action_Args = new();

        public InputAction(bool async, Action<object[]?> expression, object[] args, Key key)
        {
            ExecuteAsynchronously = async;
            Action_Args.Item1 = expression;
            Action_Args.Item2 = args;
            Key = key; 
        }
        internal void Invoke() => Action_Args.Item1?.Invoke(Action_Args.Item2);
        internal async Task InvokeAsync(float? delay = null)
        {
            if (delay is not null)
                await Task.Delay((int)delay);
             await Task.Run(() => Action_Args.Item1?.Invoke(Action_Args.Item2));
        }
    }
}