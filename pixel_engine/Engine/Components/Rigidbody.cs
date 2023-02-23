using Newtonsoft.Json;
using System;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Rigidbody : Component
    {
        private float _drag = 0.0f;
        [Field] [JsonProperty] public float drag = .6f;
        [Field] [JsonProperty] public bool usingGravity = true;
        [Field] [JsonProperty] public Vec2 velocity = new();
        [Field] [JsonProperty] public TriggerInteraction TriggerInteraction = TriggerInteraction.All; 
        
        const double dragCoefficient = 1;
        
        private protected void ApplyVelocity()
        {
            parent.Position += velocity;
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
        public static Node Standard()
        {
            Node node = Node.New;
            node.Name = $"Rigidbody Node";

            Rigidbody rb = node.AddComponent<Rigidbody>();
            Collider col = node.AddComponent<Collider>();
            Sprite sprite = node.AddComponent<Sprite>();
            col.SetVertices(sprite.GetVertices());
            sprite.color = JRandom.Color();
            col.IsTrigger = false;
            return node;
        }

        public override void Awake()
        {

        }
        public override void FixedUpdate(float delta)
        {
            if (usingGravity) 
                velocity.y += CMath.Gravity;
            ApplyDrag();
            ApplyVelocity();
        }

        public static Node StaticBody()
        {
            Node node = Rigidbody.Standard();
            Rigidbody rb = node.GetComponent<Rigidbody>();
            node.RemoveComponent(rb);
            return node; 
        }
    }
}

