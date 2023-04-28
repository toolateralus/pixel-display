using Pixel.Types.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pixel
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

        internal static List<(Line, Color)> Lines = new();
        internal static List<(Circle, Color)> Circles = new();
        internal static List<(Ray, Color)> Normals = new();
        internal static List<(Ray, Color)> Rays = new();
        [Obsolete]
        public static void FillPolygon(Polygon poly, Color color)
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
        public static void DrawPolygon(Polygon poly, Color? color = null)
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
        public static void DrawLine(Vector2 startPoint, Vector2 endPoint, Color? color = null) =>
            Lines.Add((new Line(startPoint, endPoint), color ?? Color.White));
        public static void DrawNormal(Vector2 position, Vector2 direction, Color? color = null) =>
            Normals.Add((new Ray(position, direction), color ?? Color.White));
        public static void DrawRay(Ray ray, Color? color = null) =>
            Rays.Add((ray, color ?? Color.White));
        public static void DrawCircle(Vector2 center, float radius, Color? color = null) =>
            Circles.Add((new Circle(center, radius), color ?? Color.White));
        public static void DrawCircleFilled(Vector2 center, float radius, Color? color = null)
        {
            (Circle circle, Color color) item = (new Circle(center, radius), color ?? Color.White);
            item.circle.filled = true;
            Circles.Add(item);
        }

        public static void DrawRect(Vector2 boxStart, Vector2 boxEnd, Color green)
        {
            // top bottom left right
            (Line, Color)[] lines = GetSides(boxStart, boxEnd, green);
            Lines.AddRange(lines);
        }

        private static (Line, Color)[] GetSides(Vector2 boxStart, Vector2 boxEnd, Color color)
        {
            return new (Line, Color)[]
            {
              (new(boxStart, boxStart.WithValue(x: boxEnd.X)), color),
              (new(boxEnd.WithValue(x: boxStart.X), boxEnd), color),
              (new(boxStart, boxStart.WithValue(y: boxEnd.Y)), color),
              (new(boxEnd, boxEnd.WithValue(y: boxStart.Y)), color),
            };
        }

        public static void DrawLine(Line line, Color? color = null) =>
            Lines.Add((line, color ?? Color.White));
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void DrawGraphics(RendererBase renderer, Matrix3x2 view, Matrix3x2 projection)
        {
            Matrix3x2 viewProjection = view * projection;
            Matrix3x2 viewProjScreen = viewProjection * screenMat;
            Vector2 resolution = renderer.Resolution;
            Vector2 framePos = new();
            var refColor = new Color();


            draw_filled_circles(renderer, viewProjScreen, resolution, ref framePos, ref refColor);
            draw_circles(renderer, viewProjScreen, resolution, ref framePos, ref refColor);
            draw_lines(renderer, viewProjScreen, resolution, ref framePos, ref refColor);

        }
        static void draw_filled_circles(RendererBase renderer, Matrix3x2 viewProjScreen, Vector2 resolution, ref Vector2 framePos, ref Color refColor)
        {
            foreach ((Circle circle, Color color) in Circles)
            {
                refColor = color;
                Vector2 centerPos = circle.center.Transformed(viewProjScreen) * resolution;
                float radius = circle.radius * resolution.X;
                float radiusSquared = radius * radius;

                for (int x = (int)(centerPos.X - radius); x <= centerPos.X + radius; x++)
                {
                    for (int y = (int)(centerPos.Y - radius); y <= centerPos.Y + radius; y++)
                    {
                        Vector2 pixelPos = new Vector2(x, y);
                        float distanceSquared = Vector2.DistanceSquared(centerPos, pixelPos);
                        if (distanceSquared <= radiusSquared && pixelPos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        {
                            framePos = pixelPos;
                            renderer.WriteColorToFrame(ref refColor, ref framePos);
                        }
                    }
                }
            }
        }
        static void draw_lines(RendererBase renderer, Matrix3x2 viewProjScreen, Vector2 resolution, ref Vector2 framePos, ref Color refColor)
        {
            foreach ((Line line, Color color) in Lines)
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
        static void draw_circles(RendererBase renderer, Matrix3x2 viewProjScreen, Vector2 resolution, ref Vector2 framePos, ref Color refColor)
            {
                foreach ((Circle circle, Color color) in Circles)
                {
                    if (circle.filled)
                        continue;

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
            }
        }
    }
   

