using System.Numerics;

namespace pixel_renderer
{ 
    public class Triangle : IPrimitiveGeometry
    {
        public static Vector2 DefaultSize;
        private static readonly Polygon DefaultPolygon = Polygon.Triangle(DefaultSize.X, DefaultSize.Y);

        public Polygon Polygon = DefaultPolygon;
        public Polygon DefiningGeometry => Polygon;
    }
}
