using Pixel;
using Pixel.Types.Components;
using System.Numerics;
using System.Windows.Input;
using Pixel.Types;
using static Pixel.Input;
using static Pixel.ShapeDrawer;
using static Pixel.Runtime;
namespace Pixel
{
    public class NewComponent1 : Component
    {
        bool y_pressed = false;
        public override void Dispose()
        {
        }
        public override void Awake()
        {
            RegisterAction(this, OnKeyDown_Y, Key.Y);
            RegisterAction(this, OnKeyUp_Y, Key.Y, InputEventType.KeyUp);
        }
        public override void OnDestroy()
        {
            Log($"{node.Name} has been destroyed");
        }
        private void OnKeyUp_Y()
        {
            y_pressed = false;
        }
        private void OnKeyDown_Y()
        {
            if (y_pressed)
                return;
            y_pressed = true;
        }
    }
}
