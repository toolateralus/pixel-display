using System;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using System.Numerics;
using Pixel;
using System.Threading;

namespace Pixel
{
    // todo: repair this entire class, removed a ton of behvaior while moving away from windows.
    public class CMouse
    {
        public static bool Left;
        public static bool LeftPressedLastFrame;
        public static bool LeftPressedThisFrame;
        public static Action? OnLeftPressedThisFrame;
        public static bool RightPressedThisFrame;
        public static bool RightPressedLastFrame { get; set; }
        public static bool Middle;
        public static bool Right;
        public static bool XButton1;
        public static bool XButton2;
     
        public static int MouseWheelDelta { get; set; }
        
        public static Vector2 LastClickPosition { get; set; }
        public static Vector2 Position;
        public static Vector2 LastClickGlobalPosition { get; private set; }
        public static Vector2 ScreenPosition { get; private set; }
        public static Vector2 CameraPosition { get; private set; }

        public static Vector2 GlobalPosition;
        public static Vector2 LastPosition { get; set; }
        public static Vector2 Delta { get { return LastPosition - Position; } }

        private static CMouse current = null;
        public static Action? OnMouseMove;
        
        public static CMouse Current
        {
            get
            {
                current ??= new();
                return current;
            }
        }
        public static void Update()
        {
            right_pressedThisFrame();
            left_pressedThisFrame();
            mouse_wheelDelta();
            static void mouse_wheelDelta()
            {
                if (Math.Abs(CMouse.MouseWheelDelta) == 1)
                    CMouse.MouseWheelDelta = 0;
                else CMouse.MouseWheelDelta -= CMouse.MouseWheelDelta / 2;
            }
            static void right_pressedThisFrame()
            {
                if (!RightPressedLastFrame && Right)
                    RightPressedThisFrame = true;
                else
                    RightPressedThisFrame = false;
                RightPressedLastFrame = Right;
            }
            static void left_pressedThisFrame()
            {
                if (!LeftPressedLastFrame && Left)
                {
                    LeftPressedThisFrame = true;
                    LastClickPosition = Position;
                    
                    if(Camera.First != null)
                    
                        LastClickGlobalPosition = Camera.First.ScreenToGlobal(new Vector2(0f, 0f));

                    OnLeftPressedThisFrame?.Invoke();
                }
                else
                    LeftPressedThisFrame = false;
                LeftPressedLastFrame = Left;
            }
        }
    }
}