using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace pixel_renderer
{
    public static class ShapeDrawer
    {
        public static void Refresh(Stage stage)
        {
            lines.Clear();
            circles.Clear();
            var components = stage.GetAllComponents<Component>();
            lock (components)
                foreach (Component component in components)
                    component.OnDrawShapes();

        }

        /// <summary>
        /// start point of each line will always have an x value less than or equal to the end point
        /// </summary>
        internal static List<Line> lines = new();
        internal static List<Circle> circles = new();
        public static void DrawLine(Vec2 startPoint, Vec2 endPoint, Color? color = null) =>
            lines.Add(new Line(startPoint, endPoint, color ?? Color.White));
        public static void DrawCircle(Vec2 center, float radius, Color? color = null) =>
            circles.Add(new Circle(center, radius, color ?? Color.White));
    }
    public class Line
    {
        public Color color;
        public Vec2 startPoint;
        public Vec2 endPoint;
        public Line(Vec2 startPoint, Vec2 endPoint, Color color)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.color = color;
        }
    }
    public class Circle
    {
        public Color color;
        public Vec2 center;
        public float radius;
        public Circle(Vec2 center, float radius, Color color)
        {
            this.color = color;
            this.center = center;
            this.radius = radius;
        }
    }
}
