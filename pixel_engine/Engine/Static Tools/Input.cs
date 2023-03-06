﻿using System.Collections.Generic;
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
        static Vector2 moveVector;
        static float inputMagnitude;
        private static bool moveVectorInitialized;
        public static Vector2 MoveVector { get => moveVector; }
        public static float InputMagnitude { get => inputMagnitude; set => inputMagnitude = value; }
        public static void AddInputResultToQueue(bool val, Key key)
        {

        }

        static void Up() => moveVector = new Vector2(moveVector.X, inputMagnitude);
        static void Down() => moveVector = new Vector2(moveVector.X, inputMagnitude);
        static void Left()
        {
            moveVector = new Vector2(-inputMagnitude, moveVector.Y);
        }
        static void Right()
        {
            moveVector = new Vector2(inputMagnitude, moveVector.Y);

        }

        static void InitializePlayerMoveVector()
        {
            if (moveVectorInitialized)
                return;
            
            moveVectorInitialized = true;
            
            RegisterAction(Up, Key.W);
            RegisterAction(Down, Key.S);
            RegisterAction(Left, Key.A);
            RegisterAction(Right, Key.D);
        }
        private static readonly List<InputAction> InputActions = new(250);
        internal static void Refresh()
        {
            InitializePlayerMoveVector(); 
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

        /// <summary>
        /// Gets the value of a specified key and type of event.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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