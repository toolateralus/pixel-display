using System.Numerics;

namespace pixel_renderer
{
    public class Floor : Component
    {
        public const float height = 2500;
        public const float width = 5500;
        private static Vector2 startPosition = new(-(width / 2), 0);

        public static Node Standard()
        {
            Node node = new("Floor")
            {
                Position = startPosition
            };

            node.AddComponent<Floor>();

            Collider col = node.AddComponent<Collider>();

            col.untransformedPolygon = Polygon.Rectangle(width, 250);
            col.drawCollider = true;
            col.drawNormals = true;
            return node;
        }

    }
}
