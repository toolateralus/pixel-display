using Pixel;
using Pixel.Types.Components;
using System.Numerics;
using System.Windows.Input;
using static Pixel.Input;
using static Pixel.ShapeDrawer;
using static Pixel.Runtime;
using Pixel.Types.Physics;

namespace Pixel
{
    public class CameraController : Component
    {
        Curve linear = Curve.Linear(Vector2.Zero, Vector2.One, 1, 64);
        Curve linear_exp = Curve.LinearExponential(Vector2.Zero, Vector2.One, 1, 64);
        Curve circular = Curve.Circlular(16, 64, 1, true);
        
        Node camera;
        private bool f_pressed_this_frame;

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
        public override void OnDrawShapes()
        {
            var color = ExtensionMethods.Lerp(Color.White, System.Drawing.Color.Yellow, 0.3f);
            color.a = 2;

            if (Get(Key.I))
                for (int i = 0; i < 16; ++i)
                    DrawCircleFilled(Position, i * 0.003f, color * i);
        }
    }
}
