using Pixel;
using Pixel.Types.Components;
using System.Numerics;
using System.Windows.Input;
using static Pixel.Input;
using static Pixel.ShapeDrawer;
using static Pixel.Runtime;
using System;

namespace Pixel
{
    public class CameraController : Component
    {
        Node camera;
        ParticleSystem ps;
        Vector2 vel = Vector2.Zero;
        public override void Dispose()
        {
            camera = null;
            ps = null;
        }
        public override void Awake()
        {
            Camera.First?.node?.Destroy();
            camera = new("camera");
            camera.AddComponent<Camera>();
            camera.Scale = new(16, 9);
            camera.Position = Position + Vector2.One;
            ps = node.AddComponent<ParticleSystem>();
            node.Child(camera);


            RegisterAction(this, up, Key.W);
            RegisterAction(this, down, Key.S);
            RegisterAction(this, left, Key.A);
            RegisterAction(this, right, Key.D);
            RegisterAction(this, shoot, Key.G);
        }

        private void shoot()
        {
        }

        public override void FixedUpdate(float delta)
        {
            Position += vel;

            if (vel != Vector2.Zero)
                vel *= 0.9f;

        }
        private void down()
        {
            vel += new Vector2(0, 1) / 4;
        }
        private void left()
        {
            vel += new Vector2(-1, 0) / 4;
        }
        private void right()
        {
            vel += new Vector2(1, 0) / 4;
        }
        private void up()
        {
            vel += new Vector2(0, -1) / 4;
        }
    }
}
