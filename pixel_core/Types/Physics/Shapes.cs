using System.Numerics;

namespace Pixel.Types.Physics
{
    /// <summary>
    /// A basic shape mostly used for drawing debug shapes.
    /// </summary>
    public class Line
    {
        public Vector2 EndOffset => endPoint - startPoint;
        public Vector2 startPoint;
        public Vector2 endPoint;
        public Line(Vector2 startPoint, Vector2 endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }
    }
    /// <summary>
    /// A basic shape mostly used for drawing debug shapes.
    /// </summary>
    public class Circle
    {
        public Vector2 center;
        public float radius;
        public bool filled = false;

        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }
}
