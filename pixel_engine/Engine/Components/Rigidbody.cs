﻿using Newtonsoft.Json;
namespace pixel_renderer
{
    public class Rigidbody : Component
    {
        [JsonProperty] private float _drag = 0.0f;
        [Field] [JsonProperty] public float drag = .4f;
                
        [Field] [JsonProperty] public bool usingGravity = true;
        [Field] [JsonProperty] public bool IsTrigger = false;

        [Field] [JsonProperty] public Vec2 velocity = new();
        [Field]  public Sprite? sprite; 

        const double dragCoefficient = 1;

        public TriggerInteraction TriggerInteraction = TriggerInteraction.All; 

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

