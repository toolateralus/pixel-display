using System.Numerics;

namespace pixel_renderer
{
    public class Floor : Component
    {
        public const float height = 250;
        public const float width = 5500;
        private static Vector2 startPosition = new(0, height / 2);
        private static Vector2 size = new(width, height);

        public static Node Standard()
        {
            Node node = new("Floor")
            {
                Position = startPosition
            };

            node.AddComponent<Floor>();

            Collider col = node.AddComponent<Collider>();

            col.model = Polygon.Rectangle(size);
            col.drawCollider = true;
            col.drawNormals = true;
            return node;
        }

    }
}
