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
            if (sprite == null) throw new System.Exception($"Cannot use a rigidbody without a sprite. NODE: {parentNode.Name} UUID{ parentNode.UUID}");
        }
        public override void FixedUpdate(float delta)
        {
            if (usingGravity) velocity.y += CMath.Gravity;
            Collision.ViewportCollision(parentNode);
            _drag = (float)GetDrag().Clamp(-drag, drag);
            ApplyVelocity();
            ApplyPosition();
        }


        public double GetDrag()
        {
            double velocity = this.velocity.Length;
            double drag = velocity * velocity * dragCoefficient;
            // maybe unneccesary negate?
            if (drag < 0) drag = -drag;
            return drag;
        }
        [Obsolete]
        public string GetDebugs()
        {
            return $" \n VELOCITY__X = {velocity.x} \n VELOCITY__Y = {velocity.y} \n POSITION__X = {parentNode.position.x} \n POSITION__Y {parentNode.position.y} \n NODE : {parentNode.Name}";
        }
        private protected void ApplyPosition()
        {
            parentNode.position.y += velocity.y;
            parentNode.position.x += velocity.x;
        }
        private protected void ApplyVelocity()
        {
            velocity.y *= _drag;
            velocity.x *= _drag;
        }

    }
}

