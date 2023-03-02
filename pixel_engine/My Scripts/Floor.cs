﻿using System.Numerics;

namespace pixel_renderer
{
    public class Floor : Component
    {
        public const float height = 10;
        public const float width = 5500;
        Vector2 zero = Vector2.Zero;
        private static Vector2 startPosition = new(-(width / 2), 0);
        private Polygon poly;

        public static Node Standard()
        {
            Node node = new("Floor");
            var floor = node.AddComponent<Floor>();
            node.Position = startPosition;
            Collider col = node.AddComponent<Collider>();
            floor.poly = Polygon.Rectangle(width, height);
            col.Polygon = floor.poly;
            col.drawCollider = true;
            col.drawNormals = true;
            return node;
        }

    }
}
