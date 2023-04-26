using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace pixel_core.Types.Shapes
{
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
    public class Circle
    {
        public Vector2 center;
        public float radius;
        public Circle(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }
}
