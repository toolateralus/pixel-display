using Pixel;
using Pixel.Types.Components;
using System.Numerics;
using System.Windows.Input;
using static Pixel.Input;
using static Pixel.ShapeDrawer;
using static Pixel.Runtime;
using Pixel.Types.Physics;
using System.Web;

namespace Pixel
{
    public class CameraController : Component
    {
        bool pressed = false;
        bool pressedThisFrame = false;
        Key key = Key.Y;

        [Field] float magnitude = 5f;

        Curve linear = Curve.Linear(Vector2.Zero, Vector2.One, 1, 64);
        Curve linear_exp = Curve.LinearExponential(Vector2.Zero, Vector2.One, 1, 64);
        Curve circular = Curve.Circlular(16, 64, 1, true);
        
        Node camera;
        public override void Dispose()
        {
            camera = null;
        }

        public override void Awake()
        {
            
            Camera.First?.node?.Destroy();
            camera = new("camera");
            camera.AddComponent<Camera>();
            camera.Scale = new(16, 9);
            node.Child(camera);

        }
        public override void FixedUpdate(float delta)
        {
            if (node.TryRemoveChild(camera))
            {
                if (Get(Key.C))
                    camera.Position = circular.GetValue(true);

                if (Get(Key.L) && Get(Key.LeftShift))
                    camera.Position = linear_exp.GetValue(true);

                else if (Get(Key.L))
                    camera.Position = linear.GetValue(true);

                node.Child(camera);
            }
        }
    }
}
