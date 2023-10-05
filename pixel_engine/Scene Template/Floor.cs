using System.Numerics;
using Pixel.Types.Components;
using Pixel.Types.Physics;

namespace Pixel
{
    public class Floor
    {
        private const float height = 150;
        private const float width = 5500;
        private static Vector2 startPosition = new(0, height / 2);
        private static Vector2 size = new(width, height);

        public static Node Standard()
        {
            Node node = new("Floor");
            
            node.Position = startPosition;
            node.Scale = size;
            
            node.AddComponent<Sprite>();

            Collider col = node.AddComponent<Collider>();
            col.SetModel(Polygon.Rectangle(size.Normalized()));

            return node;
        }

    }
}
