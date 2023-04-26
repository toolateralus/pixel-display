﻿using System.Numerics;
using pixel_core.Types.Components;
using pixel_core;

namespace pixel_core
{
    public class Floor : Component
    {
        public const float height = 250;
        public const float width = 5500;
        private static Vector2 startPosition = new(0, height / 2);
        private static Vector2 size = new(width, height);
        public override void Dispose()
        {
        }
        public static Node Standard()
        {
            Node node = new("Floor")
            {
                Position = startPosition
            };

            node.AddComponent<Floor>();

            Collider col = node.AddComponent<Collider>();
            col.SetModel(Polygon.Rectangle(size));
            col.drawCollider = true;
            col.drawNormals = true;

            return node;
        }

    }
}