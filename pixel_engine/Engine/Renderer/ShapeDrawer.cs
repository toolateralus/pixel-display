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
        public static event Action? DrawShapeActions;
        public static void Refresh(Stage stage)
        {
            Lines.Clear();
            Circles.Clear();
            var components = stage.GetAllComponents<Component>();
            lock (components)
                foreach (Component component in components)
                    component.OnDrawShapes();
            DrawShapeActions?.Invoke();
        }

        /// <summary>
        /// start point of each line will always have an x value less than or equal to the end point
        /// </summary>
        internal static List<Line> Lines = new();
        internal static List<Circle> Circles = new();
        public static void DrawLine(Vec2 startPoint, Vec2 endPoint, Pixel? color = null) =>
            Lines.Add(new Line(startPoint, endPoint, color ?? Pixel.White));
        public static void DrawCircle(Vec2 center, float radius, Pixel? color = null) =>
            Circles.Add(new Circle(center, radius, color ?? Pixel.White));

        public static void DrawRect(Vec2 boxStart, Vec2 boxEnd, Pixel green)
        {
            // top bottom left right
            Line[] lines = GetSides(boxStart, boxEnd, green);
            Lines.AddRange(lines);
        }

        private static Line[] GetSides(Vec2 boxStart, Vec2 boxEnd, Pixel green)
        {
            return new Line[]
            {
              new(boxStart, boxStart.WithValue(x: boxEnd.x), green),
              new(boxEnd.WithValue(x: boxStart.x), boxEnd, green),
              new(boxStart, boxStart.WithValue(y: boxEnd.y), green),
              new(boxEnd, boxEnd.WithValue(y: boxStart.y), green),
            };
        }

        public static void DrawLine(Line line) =>
            Lines.Add(line);
    }
    public class Line
    {
        public Vec2 Direction => endPoint - startPoint;
        public Pixel color;
        public Vec2 startPoint;
        public Vec2 endPoint;
        public Line(Vec2 startPoint, Vec2 endPoint, Pixel? color = null)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.color = color ?? Pixel.White;
        }
    }
    public class Circle
    {
        public Pixel color;
        public Vec2 center;
        public float radius;
        public Circle(Vec2 center, float radius, Pixel color)
        {
            this.color = color;
            this.center = center;
            this.radius = radius;
        }
    }
}
