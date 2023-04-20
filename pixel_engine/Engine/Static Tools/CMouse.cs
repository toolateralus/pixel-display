using System;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
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


        public CMouse(MouseButtonEventArgs? e = null)
        {
            if (current != this)
                current = null;

            current = this;

            if(e is not null)
                Refresh(e);

        }

        public static void Update()
        {

            if (!RightPressedLastFrame && Right)
                RightPressedThisFrame = true;
            else
                RightPressedThisFrame = false;
            RightPressedLastFrame = Right;

            if (!LeftPressedLastFrame && Left)
            {
                LeftPressedThisFrame = true;
                LastClickPosition = Position;
                
                var img = Runtime.OutputImages.First();
                var normalizedPos = img.GetNormalizedPoint(new Point(LastClickPosition.X, LastClickPosition.Y));

                LastClickGlobalPosition = Camera.First.ScreenToGlobal(new Vector2((float)normalizedPos.X, (float)normalizedPos.Y));
                OnLeftPressedThisFrame?.Invoke();
            }
            else
                LeftPressedThisFrame = false;
            LeftPressedLastFrame = Left;

        }

        public static void Refresh(MouseButtonEventArgs e)
        {
            e.Handled = true;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    Left = e.LeftButton is MouseButtonState.Pressed;
                    break;
                case MouseButton.Middle:
                    Middle = e.MiddleButton is MouseButtonState.Pressed;
                    break;
                case MouseButton.Right:
                    Right = e.RightButton is MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton1:
                    XButton1 = e.XButton1 is MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton2:
                    XButton2 = e.XButton2 is MouseButtonState.Pressed;
                    break;
            }
        }
        public static void Refresh(MouseWheelEventArgs e)
        {
            e.Handled = true;
            MouseWheelDelta = e.Delta;
        }
        public static void Refresh(MouseEventArgs e)
        {
            e.Handled = true; 
            LastPosition = Position;
            var img = Runtime.OutputImages.First();
            var point = e.GetPosition(img);
            Position = new((float)point.X, (float)point.Y);
            var pt = img.GetNormalizedPoint(point);
            if (Camera.First is not Camera firstCam)
                return;
            ScreenPosition = pt.ToVector2();
            CameraPosition = firstCam.ScreenToLocal(ScreenPosition);
            GlobalPosition = firstCam.ScreenToGlobal(ScreenPosition);
            OnMouseMove?.Invoke();
        }
    }
}