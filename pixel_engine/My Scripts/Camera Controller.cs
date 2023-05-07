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
        [Field] float speed = 0.15f;

        public override void Dispose()
        {
            camera = null;
            ps = null;
        }
        public override void Awake()
        {
            camera = new("camera2");
            Camera.First = camera.AddComponent<Camera>();
            camera.Scale = new(16, 9);
            camera.Position = Position + Vector2.One;
            node.Child(camera);


            RegisterAction(this, up, Key.W);
            RegisterAction(this, down, Key.S);
            RegisterAction(this, left, Key.A);
            RegisterAction(this, right, Key.D);
        }

        public override void FixedUpdate(float delta)
        {
            const int divisorA = 10;
            const int divisorB = divisorA * 10;

            if (camera is null)
                return;

            float velocity = Math.Abs(((speed + Math.Min((vel.Length() / divisorA), 1) - camera.Scale.Length() / divisorB)));
            
            if (Get(Key.LeftShift))
                velocity *= 0.5f;

            Position += vel * velocity;

            vel *= 0.9f;

        }
        private void down()
        {
            vel += new Vector2(0, speed);
        }
        private void left()
        {
            vel += new Vector2(-speed, 0);
        }
        private void right()
        {
            vel += new Vector2(speed, 0);
        }
        private void up()
        {
            vel += new Vector2(0, -speed);
        }
    }
}
