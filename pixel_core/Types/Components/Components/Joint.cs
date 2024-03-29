﻿using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Numerics;

namespace Pixel
{
    [Serializable]
    public class Joint : Component
    {
        public Node A;
        public Node B;
        internal Vector2? offset;

        [Field] string NameB = "Player";
        public override void Dispose()
        {
            A = null;
            B = null;
        }
        [Method]
        public void AttachBodiesByName()
        {
            A = node;
            if (Interop.Stage is Stage stage)
            {
                var b = stage.FindNode(NameB);
                if (b is null)
                {
                    Interop.Log($"Body B of Joint on {node.Name} was not set. No connection was made.");
                    return;
                }
                Interop.Log($" node {b.Name} Connected to node {node.Name}.");

                offset = b.Position - node.Position;

                if (node.rb != null && b.rb != null)
                {
                    b.RemoveComponent(b.rb);
                    B = b;

                    A.OnCollided += (c) => OnBodyCollided(c, 0);
                    B.OnCollided += (c) => OnBodyCollided(c, 1);
                }
            }
        }
        private void OnBodyCollided(Collision obj, int body)
        {
            switch (body)
            {
                case 0:
                    break;

                case 1:
                    if (offset.HasValue)

                        A.Position = B.Position - offset.Value;
                    A.rb.velocity = Vector2.Zero;
                    break;
            }
        }
        public override void FixedUpdate(float delta)
        {
            if (A is not null && B is not null && offset.HasValue)
                B.Position = A.Position + offset.Value;

        }
    }
}