using System.Numerics;

namespace pixel_renderer
{
    public class Floor : Component
    {
        public const float height = 2500;
        public const float width = 5500;
        private static Vector2 startPosition = new(-(width / 2), 0);
        private Polygon poly;

        public static Node Standard()
        {
            Node node = new("Floor");
            node.Position = startPosition;


            var floor = node.AddComponent<Floor>();
            Collider col = node.AddComponent<Collider>();

            floor.poly = Polygon.Rectangle(width, height);
            col.untransformedPolygon = floor.poly;
            col.drawCollider = true;
            col.drawNormals = true;
            return node;
        }

    }
}
