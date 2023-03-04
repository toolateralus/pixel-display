using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Rigidbody : Component
    {


        [Field][JsonProperty] public float mass = 1f;
        [Field][JsonProperty] public float invMass;
        [Field][JsonProperty] public float gravityFactor;
        [Field][JsonProperty] public Vector2 velocity = Vector2.Zero;
        [Field][JsonProperty] public Vector2 acceleration = Vector2.Zero;
        [Field][JsonProperty] public float restitution = 2f;
        [Field][JsonProperty] public float drag = .1f;
        [Field][JsonProperty] public bool usingGravity = true;
        [Field][JsonProperty] public TriggerInteraction TriggerInteraction = TriggerInteraction.All;
        const double dragCoefficient = 1;

        public override void FixedUpdate(float deltaTime)
        {
            velocity += CMath.Gravity; 

            velocity += acceleration;

            velocity *= 0.99f;

            Position += velocity; 

            velocity *= 1f / (1f + 0.01f * (drag * MathF.Abs(Vector2.Dot(velocity.Normalized(), acceleration.Normalized()))));
        }
        public override void Awake()
        {
            invMass = 1f / mass;
        }
        public void ApplyImpulse(Vector2 impulse)
        {
            velocity += impulse * invMass;
        }
        public static Node Standard(string v = "Rigidbody Node ")
        {
            Node node = Node.New;
            string tag = $"{JRandom.Hexadecimal()}{JRandom.Hexadecimal()}{JRandom.Hexadecimal()}{JRandom.Hexadecimal()}";
            node.Name = $"{v} {tag}";

            Rigidbody rb = node.AddComponent<Rigidbody>();
            Collider col = node.AddComponent<Collider>();
            Sprite sprite = node.AddComponent<Sprite>();
            col.SetVertices(sprite.GetVertices());
            //sprite.color = JRandom.Color(aMin: 128);
            col.IsTrigger = false;
            return node;
        }
        public static Node Standard()
        {
            Node node = Node.New;
            string tag = $"{JRandom.Hexadecimal()}{JRandom.Hexadecimal()}{JRandom.Hexadecimal()}{JRandom.Hexadecimal()}";
            node.Name = $"Rigidbody Node {tag}";

            Rigidbody rb = node.AddComponent<Rigidbody>();
            Collider col = node.AddComponent<Collider>();
            Sprite sprite = node.AddComponent<Sprite>();
            col.SetVertices(sprite.GetVertices());
            //sprite.color = JRandom.Color(aMin: 128);
            col.IsTrigger = false;
            return node;
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

