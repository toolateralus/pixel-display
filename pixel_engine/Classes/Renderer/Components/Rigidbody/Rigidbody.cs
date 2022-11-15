using pixel_renderer;
using System;

namespace pixel_renderer
{
    public class Rigidbody : Component
    {
        private float _drag = 0.0f;
        public float drag = .2f;
        public bool usingGravity = true;
        public Vec2 velocity = new();
        private Sprite? sprite;
        const double dragCoefficient = 1;
        public bool IsTrigger { get; internal set; } = false; 

        public override void Awake()
        {
            base.Awake();
            sprite = parentNode.GetComponent<Sprite>();
            if (sprite == null) throw new Exception($"Cannot use a rigidbody without a sprite. NODE: {parentNode.Name} UUID{ parentNode.UUID}");
        }
        public override void FixedUpdate(float delta)
        {
            if (usingGravity) velocity.y += CMath.Gravity;
            Collision.ViewportCollision(parentNode);
            ApplyDrag();
            ApplyVelocity();
        }

        public double GetDrag()
        {
            double velocity = this.velocity.Length;
            double drag = velocity * velocity * dragCoefficient;
            // maybe unneccesary negate?
            if (drag < 0) drag = -drag;
            return drag;
        }

        private protected void ApplyVelocity()
        {
            parentNode.position.y += velocity.y;
            parentNode.position.x += velocity.x;
        }

        private protected void ApplyDrag()
        {
            _drag = (float)GetDrag().Clamp(-drag, drag);  
            velocity.y *= _drag;
            velocity.x *= _drag;
        }

    }
}

