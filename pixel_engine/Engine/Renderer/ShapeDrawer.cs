using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer.ShapeDrawing
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
        public static void DrawLine(Vector2 startPoint, Vector2 endPoint, Pixel? color = null) =>
            Lines.Add(new Line(startPoint, endPoint, color ?? Pixel.White));
        public static void DrawCircle(Vector2 center, float radius, Pixel? color = null) =>
            Circles.Add(new Circle(center, radius, color ?? Pixel.White));

        public static void DrawRect(Vector2 boxStart, Vector2 boxEnd, Pixel green)
        {
            // top bottom left right
            Line[] lines = GetSides(boxStart, boxEnd, green);
            Lines.AddRange(lines);
        }

        private static Line[] GetSides(Vector2 boxStart, Vector2 boxEnd, Pixel green)
        {
            return new Line[]
            {
              new(boxStart, boxStart.WithValue(x: boxEnd.X), green),
              new(boxEnd.WithValue(x: boxStart.X), boxEnd, green),
              new(boxStart, boxStart.WithValue(y: boxEnd.Y), green),
              new(boxEnd, boxEnd.WithValue(y: boxStart.Y), green),
            };
        }

        public static void DrawLine(Line line) =>
            Lines.Add(line);
    }
    public class Line
    {
        public Vector2 Direction => endPoint - startPoint;
        public Pixel color;
        public Vector2 startPoint;
        public Vector2 endPoint;
        public Line(Vector2 startPoint, Vector2 endPoint, Pixel? color = null)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.color = color ?? Pixel.White;
        }
    }
    public class Circle
    {
        public Pixel color;
        public Vector2 center;
        public float radius;
        public Circle(Vector2 center, float radius, Pixel color)
        {
            this.color = color;
            this.center = center;
            this.radius = radius;
        }
    }
}
