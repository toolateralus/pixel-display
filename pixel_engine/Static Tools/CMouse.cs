using System;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using System.Numerics;
using Pixel;
using System.Threading;

namespace Pixel
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

        public static WeakReference<System.Windows.Controls.Image> last_image;
        public static void FocusImage(System.Windows.Controls.Image image)
        {
            if (last_image is not null)
                if (last_image.TryGetTarget(out var lastImageRef))
                { 
                    if (lastImageRef is System.Windows.Controls.Image img)
                    {
                        img.MouseDown -= Image_MouseBtnChanged;
                        img.MouseUp -= Image_MouseBtnChanged;
                        img.MouseMove -= Image_MouseMove;
                        img.MouseWheel -= OnMouseWheelMoved;
                    }
                }

            last_image = new(image);
            image.MouseDown += Image_MouseBtnChanged;
            image.MouseUp += Image_MouseBtnChanged;
            image.MouseMove += Image_MouseMove;
            image.MouseWheel += OnMouseWheelMoved;

        }
        public static void OnMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            MouseWheelRefresh(e);
        }
        private static void Image_MouseMove(object sender, MouseEventArgs e)
        {
            RefreshMousePosition(e);
        }
        private static void Image_MouseBtnChanged(object sender, MouseButtonEventArgs e)
        {
            MouseButtonRefresh(e);
        }
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
                MouseButtonRefresh(e);
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

                    var img = Runtime.OutputImages.First();
                    var normalizedPos = img.GetNormalizedPoint(new Point(LastClickPosition.X, LastClickPosition.Y));

                    if(Camera.First != null)
                        LastClickGlobalPosition = Camera.First.ScreenToGlobal(new Vector2((float)normalizedPos.X, (float)normalizedPos.Y));

                    OnLeftPressedThisFrame?.Invoke();
                }
                else
                    LeftPressedThisFrame = false;
                LeftPressedLastFrame = Left;
            }
        }
        public static void MouseButtonRefresh(MouseButtonEventArgs e)
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
        public static void MouseWheelRefresh(MouseWheelEventArgs e)
        {
            e.Handled = true;
            MouseWheelDelta += e.Delta;
        }
        public static void RefreshMousePosition(MouseEventArgs e)
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