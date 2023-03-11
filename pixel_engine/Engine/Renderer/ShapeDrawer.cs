using System;
using System.Collections.Generic;
using System.Linq;
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
            {
                for (int i = 0; i < components.Count(); ++i)
                {
                    var component = components.ElementAt(i);
                    component.OnDrawShapes();
                }
            }
            DrawShapeActions?.Invoke();
        }

        internal static List<(Line, Pixel)> Lines = new();
        internal static List<(Circle, Pixel)> Circles = new();
        public static void DrawLine(Vector2 startPoint, Vector2 endPoint, Pixel? color = null) =>
            Lines.Add((new Line(startPoint, endPoint), color ?? Pixel.White));
        public static void DrawCircle(Vector2 center, float radius, Pixel? color = null) =>
            Circles.Add((new Circle(center, radius), color ?? Pixel.White));

        public static void DrawRect(Vector2 boxStart, Vector2 boxEnd, Pixel green)
        {
            // top bottom left right
            (Line, Pixel)[] lines = GetSides(boxStart, boxEnd, green);
            Lines.AddRange(lines);
        }

        private static (Line, Pixel)[] GetSides(Vector2 boxStart, Vector2 boxEnd, Pixel color)
        {
            return new (Line, Pixel)[]
            {
              (new(boxStart, boxStart.WithValue(x: boxEnd.X)), color),
              (new(boxEnd.WithValue(x: boxStart.X), boxEnd), color),
              (new(boxStart, boxStart.WithValue(y: boxEnd.Y)), color),
              (new(boxEnd, boxEnd.WithValue(y: boxStart.Y)), color),
            };
        }

        public static void DrawLine(Line line, Pixel? color = null) =>
            Lines.Add((line, color ?? Pixel.White));
    }
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
