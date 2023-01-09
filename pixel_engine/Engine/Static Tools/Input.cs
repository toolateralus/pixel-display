using System.Collections.Generic;
using System;
using System.Security.Principal;

namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Input;
    using static System.Runtime.CompilerServices.RuntimeHelpers;

    public static class Input
    {
        public enum InputEventType { KeyDown, KeyUp, KeyToggle };
        static Dictionary<Key, bool> KeyDown = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeyUp = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> KeyToggled = new Dictionary<Key, bool>();

        readonly static Key[] keys =
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
        static List<InputAction> InputActions_KeyToggle = new(101);

        public static void RegisterAction(InputAction action, InputEventType type)
        {
            switch (type)
            {
                case InputEventType.KeyDown:
                    InputActions_KeyDown.Add(action);
                    break;

                case InputEventType.KeyUp:
                    throw new NotImplementedException();

                case InputEventType.KeyToggle:
                    throw new NotImplementedException();
            }
        }

        public static event Action<Key> OnKeyDown;
        public static bool InputActionsInUse = true;
        public static void Refresh()
        {

            if (!InputActionsInUse)
                UpdateStaticInputs();
            else
            {
                IterateActions(InputActions_KeyDown);
                
                // Not yet implemented; NYI TODO
                //IterateActions(InputActions_KeyUp);
                //IterateActions(InputActions_KeyToggle);
            }

        }

        private static void IterateActions(List<InputAction> actions)
        {
            foreach (var input in actions)
            {
                Key key = input.Key;
                SetKeyDown(key, Keyboard.IsKeyDown(key));
                SetKeyUp(key, Keyboard.IsKeyUp(key));
                SetKeyToggled(key, Keyboard.IsKeyToggled(key));
            }
        }

        private static void UpdateStaticInputs()
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
            var result = KeyDown.ContainsKey(key) && KeyDown[key];

            return result;
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

        internal static void Awake()
        {
            OnKeyDown += Input_OnKeyDown;
            
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
                    EditorMessage msg = new("$editor Debug.Log('Message') $end");
                    Runtime.RaiseInspectorEvent(msg);
                }
        }
    }
    public class EditorMessage : InspectorEvent
    {
        public EditorMessage(string msg)
        {
            message = msg;
        }
    }

    public class InputAction
    {
        internal readonly bool ExecuteAsynchronously = false;
        internal Key Key; 
        
        private ValueTuple<Action<object[]?>, object[]?> Action_Args = new();
        
        internal void Invoke() => Action_Args.Item1?.Invoke(Action_Args.Item2);
        internal async Task InvokeAsync(float? delay = null)
        {
            if (delay is not null)
                await Task.Delay((int)delay);
             await Task.Run(() => Action_Args.Item1?.Invoke(Action_Args.Item2));
        }
        
        public InputAction(bool async, Action<object[]?> expression, object[] args, Key key)
        {
            ExecuteAsynchronously = async;
            Action_Args.Item1 = expression;
            Action_Args.Item2 = args;
            Key = key; 
        }
    }
}