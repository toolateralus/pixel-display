using Pixel;
using Pixel.Types.Components;
using System.Numerics;
using System.Windows.Input;
using Pixel.Types;
using static Pixel.Input;
using static Pixel.ShapeDrawer;
using static Pixel.Runtime;
using System;
using System.Windows.Shell;

namespace Pixel
{
    public class Fire : Component
    {
        bool pressed = false;
        bool pressedThisFrame = false;
        Key key = Key.Y;
        public override void Dispose()
        {

        }
        public override void Awake()
        {
            
            Camera.First?.node?.Destroy();

            Node camera = new("thisCamera");
            camera.AddComponent<Camera>();
            node.Child(camera);



            RegisterAction(this, OnKeyDown_Y, key);
            RegisterAction(this, OnKeyUp_Y, key, InputEventType.KeyUp);
        }
        public override void FixedUpdate(float delta)
        {
            if(pressedThisFrame)




            pressedThisFrame = false;
        }
        private void OnKeyUp_Y()
        {
            pressed = false;
        }
        private void OnKeyDown_Y()
        {
            pressed = true;
            pressedThisFrame = true;
        }
    }
}
