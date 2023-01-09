using Newtonsoft.Json;
using System;
using System.IO.Packaging;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;

namespace pixel_renderer
{
    public class Rigidbody : Component
    {
        [JsonProperty] private float _drag = 0.0f;
        [JsonProperty] public float drag = .2f;

        [JsonProperty] public bool usingGravity = true;
        [JsonProperty] public bool IsTrigger = false;

        [JsonProperty] public Vec2 velocity = new();
        [JsonProperty] public Sprite? sprite; 

        const double dragCoefficient = 1;
        
        private protected void ApplyVelocity()
        {
            parent.position.y += velocity.y;
            parent.position.x += velocity.x;
        }
        private protected void ApplyDrag()
        {
            _drag = (float)GetDrag().Clamp(-drag, drag);
            velocity.y *= _drag;
            velocity.x *= _drag;
        }
        private protected double GetDrag()
        {
            double velocity = this.velocity.Length();
            double drag = velocity * velocity * dragCoefficient;
            // maybe unneccesary negate?
            // breakpoint has never been hit despite
            // running in debug for dozens of hours
            if (drag < 0) drag = -drag;
            return drag;
        }

        // Todo: prevent these methods from being overridden.
        // Immutable Method attribute or something.
        public override void Awake() => parent.TryGetComponent(out sprite);
        public override void FixedUpdate(float delta)
        {
            if (usingGravity) velocity.y += CMath.Gravity;
            ApplyDrag();
            ApplyVelocity();
        }
        
    }
}

