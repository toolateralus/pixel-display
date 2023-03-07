using System.Numerics;

namespace pixel_renderer.Engine.Primitives
{
    public class Circle : IPrimitiveGeometry
    {
        public static Vector2 DefaultSize;
        private static readonly Polygon DefaultPolygon = Polygon.Circle(DefaultSize.X, 8);

        public Polygon Polygon = DefaultPolygon;
        public Polygon DefiningGeometry => Polygon;
    }
}
