﻿using Newtonsoft.Json;
using Pixel.Statics;
using Pixel.Types.Components;
using System;
using System.Numerics;

namespace Pixel
{
    public class Rigidbody : Component
    {
        [Field][JsonProperty] public float mass = 1f;
        [Field][JsonProperty] public float invMass;
        [Field][JsonProperty] public float restitution = 0.5f;
        [Field][JsonProperty] public float drag = 1f;
        [Field][JsonProperty] public bool usingGravity = true;
        [Field][JsonProperty] public Vector2 velocity = Vector2.Zero;
        [Field][JsonProperty] public Vector2 acceleration = Vector2.Zero;
        public override void Dispose() { }
        public override void FixedUpdate(float deltaTime)
        {
            if (usingGravity)
                velocity += CMath.Gravity * deltaTime;

            velocity += acceleration;
            velocity *= 0.99f;
            float dot = Vector2.Dot(velocity.Normalized(), acceleration.Normalized());
            float force = MathF.Abs(dot);
            velocity *= 1f / (1f + 0.01f * (drag * force));

            Position += velocity;
        }
        public override void Awake()
        {
            invMass = 1f / mass;
        }
        public void ApplyImpulse(Vector2 impulse)
        {
            velocity += impulse * invMass;
        }
        public static Node Standard(string name)
        {
            Node node = new("Standard Node");
            node.Name = name;

            Rigidbody rb = node.AddComponent<Rigidbody>();
            rb.Enabled = true;

            Collider col = node.AddComponent<Collider>();

            Sprite sprite = node.AddComponent<Sprite>();
            sprite.Color = JRandom.Color(aMin: 200);

            node.Scale = Constants.DefaultNodeScale;
            return node;
        }
        public static Node Standard() =>
            Standard("Rigidbody " + JRandom.Hex());
        public static Node StaticBody()
        {
            Node node = Standard("Static Body " + JRandom.Hex());
            Rigidbody rb = node.GetComponent<Rigidbody>();
            node.RemoveComponent(rb);
            return node;
        }
    }
}

