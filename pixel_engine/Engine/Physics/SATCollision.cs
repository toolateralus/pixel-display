using System;
using System.Collections.Generic;
using System.Numerics;

namespace pixel_renderer
{
    public class SATCollision
    {
        public static Vector2 GetMinimumDepthVector(Polygon polygonA, Polygon polygonB)
        {
            if (polygonA.vertices.Length == 0 || polygonB.vertices.Length == 0)
                return Vector2.Zero;
            float overlap = float.MaxValue;
            Vector2 smallest = Vector2.Zero;

            List<Vector2> normals = new List<Vector2>(polygonA.normals);
            normals.AddRange(polygonB.normals);

            foreach (Vector2 normal in normals)
            {
                SATProjection p1 = Project(polygonA, normal);
                SATProjection p2 = Project(polygonB, normal);

                if (!Overlap(p1, p2))
                    return Vector2.Zero;
                float o = GetOverlap(p1, p2);
                if (o >= overlap)
                    continue;
                overlap = o;
                smallest = normal;
            }

            if (Vector2.Dot(polygonB.centroid - polygonA.centroid, smallest) < 0)
                smallest *= -1;
            smallest *= -1;
            return smallest * overlap;
        }
        private static float GetOverlap(SATProjection p1, SATProjection p2) => MathF.Min(p1.max, p2.max) - MathF.Max(p1.min, p2.min);
        private static SATProjection Project(Polygon polygon, Vector2 axis)
        {
            SATProjection projection = new SATProjection();
            projection.normal = axis;
            projection.min = Vector2.Dot(polygon.vertices[0], axis);
            projection.max = projection.min;

            for (int i = 1; i < polygon.vertices.Length; i++)
            {
                float p = Vector2.Dot(polygon.vertices[i], axis);
                if (p < projection.min)
                {
                    projection.min = p;
                }
                else if (p > projection.max)
                {
                    projection.max = p;
                }
            }
            return projection;
        }
        private static bool Overlap(SATProjection p1, SATProjection p2)
        {
            return (p1.min <= p2.max && p1.max >= p2.min);
        }
    }

}