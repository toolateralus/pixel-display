
using pixel_core.Types.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace pixel_core
{
    public static class ShapeDrawer
    {
        public static event Action? DrawShapeActions;
        static readonly Matrix3x2 screenMat = Matrix3x2.CreateTranslation(1, 1) * Matrix3x2.CreateScale(0.5f, 0.5f);
        public static void Refresh(Stage stage)
        {
            Lines.Clear();
            Circles.Clear();
            Rays.Clear();
            Normals.Clear();

            var nodesCount = stage.nodes.Count;
            for (int i = 0; i < nodesCount; i++)
                stage.nodes[i]?.OnDrawShapes();
            DrawShapeActions?.Invoke();
        }

        internal static List<(Line, Pixel)> Lines = new();
        internal static List<(Circle, Pixel)> Circles = new();
        internal static List<(Ray, Pixel)> Normals = new();
        internal static List<(Ray, Pixel)> Rays = new();
        [Obsolete]
        public static void FillPolygon(Polygon poly, Pixel color)
        {
            // Get the polygon edges
            List<Line> edges = poly.GetLines();

            // Get the polygon bounds
            int minY = (int)poly.vertices.Min(v => v.Y);
            int maxY = (int)poly.vertices.Max(v => v.Y);

            // Loop through each scanline
            for (int y = minY; y <= maxY; y++)
            {
                // Get the intersection points with the polygon edges
                List<Vector2> intersections = new List<Vector2>();

                for (int i = 0; i < edges.Count; i++)
                {
                    Line edge = edges[i];

                    if (edge.startPoint.Y <= y && edge.endPoint.Y > y ||
                        edge.endPoint.Y <= y && edge.startPoint.Y > y)
                    {
                        float x = (y - edge.startPoint.Y) /
                                  (edge.endPoint.Y - edge.startPoint.Y) *
                                  (edge.endPoint.X - edge.startPoint.X) +
                                  edge.startPoint.X;

                        intersections.Add(new Vector2(x, y));
                    }
                }

                // Sort the intersection points by x-coordinate
                intersections = intersections.OrderBy(p => p.X).ToList();

                // Fill the space between each pair of intersection points
                for (int i = 0; i < intersections.Count; i += 2)
                {
                    Vector2 start = intersections[i];
                    Vector2 end = intersections[i + 1];

                    DrawLine(start, end, color);
                }
            }
        }
        public static void DrawPolygon(Polygon poly, Pixel? color = null)
        {
            List<Line> lines = poly.GetLines();
            for (int i = 0; i < lines.Count; i++)
            {
                Line? line = lines[i];
                if (color.HasValue)
                {
                    DrawLine(line, color.Value);
                    continue;
                }
                DrawLine(line);
            }
        }
        public static void DrawLine(Vector2 startPoint, Vector2 endPoint, Pixel? color = null) =>
            Lines.Add((new Line(startPoint, endPoint), color ?? Pixel.White));
        public static void DrawNormal(Vector2 position, Vector2 direction, Pixel? color = null) =>
            Normals.Add((new Ray(position, direction), color ?? Pixel.White));
        public static void DrawRay(Ray ray, Pixel? color = null) =>
            Rays.Add((ray, color ?? Pixel.White));
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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void DrawGraphics(RendererBase renderer, Matrix3x2 view, Matrix3x2 projection)
        {
            Matrix3x2 viewProjection = view * projection;
            Matrix3x2 viewProjScreen = viewProjection * screenMat;
            Vector2 resolution = renderer.Resolution;
            Vector2 framePos = new();
            var refColor = new Pixel();
            foreach ((Circle circle, Pixel color) in Circles)
            {
                refColor = color;
                float sqrtOfHalf = MathF.Sqrt(0.5f);
                Vector2 radius = circle.center + new Vector2(circle.radius, circle.radius);
                Vector2 centerPos = circle.center.Transformed(viewProjScreen) * resolution;
                Vector2 pixelRadius = radius.Transformed(viewProjScreen) * resolution - centerPos;
                Vector2 quaterArc = pixelRadius * sqrtOfHalf;
                int quarterArcAsInt = (int)quaterArc.X;
                for (int x = -quarterArcAsInt; x <= quarterArcAsInt; x++)
                {
                    float y = MathF.Cos(MathF.Asin(x / pixelRadius.X)) * pixelRadius.Y;
                    framePos.X = centerPos.X + x;
                    framePos.Y = centerPos.Y + y;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref refColor, ref framePos);
                    framePos.Y = centerPos.Y - y;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref refColor, ref framePos);
                }
                quarterArcAsInt = (int)quaterArc.Y;
                for (int y = -quarterArcAsInt; y <= quarterArcAsInt; y++)
                {
                    float x = MathF.Cos(MathF.Asin(y / pixelRadius.Y)) * pixelRadius.X;
                    framePos.Y = centerPos.Y + y;
                    framePos.X = centerPos.X + x;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref refColor, ref framePos);
                    framePos.X = centerPos.X - x;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref refColor, ref framePos);
                }
            }
            foreach ((Line line, Pixel color) in Lines)
            {
                refColor = color;
                Vector2 startPos = line.startPoint.Transformed(viewProjScreen) * resolution;
                Vector2 endPos = line.endPoint.Transformed(viewProjScreen) * resolution;
                if (startPos == endPos)
                {
                    if (startPos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref refColor, ref startPos);
                    continue;
                }

                float xDiff = startPos.X - endPos.X;
                float yDiff = startPos.Y - endPos.Y;

                if (MathF.Abs(xDiff) > MathF.Abs(yDiff))
                {
                    float slope = yDiff / xDiff;
                    float yIntercept = startPos.Y - slope * startPos.X;

                    int endX = (int)MathF.Min(MathF.Max(startPos.X, endPos.X), resolution.X);

                    for (int x = (int)MathF.Max(MathF.Min(startPos.X, endPos.X), 0); x < endX; x++)
                    {
                        framePos.X = x;
                        framePos.Y = slope * x + yIntercept;
                        if (framePos.Y < 0 || framePos.Y >= resolution.Y)
                            continue;
                        renderer.WriteColorToFrame(ref refColor, ref framePos);
                    }
                }
                else
                {
                    float slope = xDiff / yDiff;
                    float xIntercept = startPos.X - slope * startPos.Y;

                    int endY = (int)MathF.Min(MathF.Max(startPos.Y, endPos.Y), resolution.Y);

                    for (int y = (int)MathF.Max(MathF.Min(startPos.Y, endPos.Y), 0); y < endY; y++)
                    {
                        framePos.Y = y;
                        framePos.X = slope * y + xIntercept;
                        if (framePos.X < 0 || framePos.X >= resolution.X)
                            continue;
                        renderer.WriteColorToFrame(ref refColor, ref framePos);
                    }
                }
            }
        }
    }
   
}
