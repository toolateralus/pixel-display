using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace pixel_renderer
{
    public enum InputEventType { KeyDown, KeyUp, KeyToggle }
    public static class Input
    {
        private static readonly List<InputAction> InputActions = new(250);
        internal static void Refresh()
        {
            lock (InputActions)
            {
                InputAction[] actions = new InputAction[InputActions.Count];
                InputActions.CopyTo(0, actions, 0, InputActions.Count); 
                foreach (var action in actions)
                {
                    var type = action.EventType;
                    bool input_value = Get(action.Key, type);
                    if (input_value)
                        if (action.ExecuteAsynchronously) Task.Run(() => action.InvokeAsync());
                        else action.Invoke();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool Get(string key, InputEventType type = InputEventType.KeyDown)
        {
            if(Application.Current is Application app)
                return app.Dispatcher.Invoke(() => { 
                    Key key_ = Enum.Parse<Key>(key);
                    var input_value = type switch
                    {
                        InputEventType.KeyDown => Keyboard.IsKeyDown(key_),
                        InputEventType.KeyUp => Keyboard.IsKeyUp(key_),
                        InputEventType.KeyToggle => Keyboard.IsKeyToggled(key_),
                        _ => false,
                    };
                    return input_value;
                });
            else return false; 
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool Get(Key key, InputEventType type = InputEventType.KeyDown)
        {
            bool input_value = false;
            Application.Current?.Dispatcher.Invoke(delegate
                {
                    try
                    {
                        input_value = type switch
                        {
                            InputEventType.KeyDown => Keyboard.IsKeyDown(key),
                            InputEventType.KeyUp => Keyboard.IsKeyUp(key),
                            InputEventType.KeyToggle => Keyboard.IsKeyToggled(key),
                            _ => false,
                        };
                    }
                    catch (Exception e)
                    {
                        Runtime.Log(e.Message);
                    }
                });
            return input_value;
        }
        public static void RegisterAction(Action action, Key key, InputEventType type = InputEventType.KeyDown)
        {
             InputActions.Add(new(action, key, type: type));
        }
    }
}