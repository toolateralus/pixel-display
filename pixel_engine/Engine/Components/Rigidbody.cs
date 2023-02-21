using Newtonsoft.Json;
using System;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Rigidbody : Component
    {
        private float _drag = 0.0f;
        [Field] [JsonProperty] public float drag = .6f;
        [Field] [JsonProperty] public bool usingGravity = false;
        [Field] [JsonProperty] public Vec2 velocity = new();
        [Field] [JsonProperty] public TriggerInteraction TriggerInteraction = TriggerInteraction.All; 
        [Field] public Sprite? sprite; 
        
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
            
            Vec2 screenSize = new(256, 256);
            

            node.Name = $"Node {JRandom.Bool()}";
            node.Position = JRandom.Vec2(Vec2.zero, screenSize);

            Rigidbody rb = node.AddComponent<Rigidbody>();
            Collider col = node.AddComponent<Collider>();

            rb.parent = node;
            rb.sprite = new(16, 16);

            node.AddComponent(rb.sprite);

            col.size = rb.sprite.size;
            col.IsTrigger = false;

            return node;
        }

        public override void Awake() => parent.TryGetComponent(out sprite);
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

