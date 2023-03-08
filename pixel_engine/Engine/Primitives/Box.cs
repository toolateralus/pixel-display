using System.Numerics;

namespace pixel_renderer
{
    public class Box : IPrimitiveGeometry
    {
        public static Vector2 DefaultSize = new(1, 1);
        public Polygon Polygon = Polygon.Rectangle(DefaultSize);
        public Polygon DefiningGeometry => Polygon;
    }
}
