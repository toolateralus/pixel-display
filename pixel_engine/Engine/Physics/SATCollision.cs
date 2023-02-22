using System;
using System.Collections.Generic;

namespace pixel_renderer
{
    public class SATCollision
    {
        public static Vec2 GetMinimumDepthVector(Polygon polygonA, Polygon polygonB)
        {
            float overlap = float.MaxValue;
            Vec2 smallest = Vec2.zero;

            List<Vec2> normals = new List<Vec2>(polygonA.normals);
            normals.AddRange(polygonB.normals);

            foreach (Vec2 normal in normals)
            {
                SATProjection p1 = Project(polygonA, normal);
                SATProjection p2 = Project(polygonB, normal);

                if (!Overlap(p1, p2))
                    return Vec2.zero;
                float o = GetOverlap(p1, p2);
                if (o >= overlap)
                    continue;
                overlap = o;
                smallest = normal;
            }

            if (Vec2.Dot(polygonB.centroid - polygonA.centroid, smallest) < 0)
                smallest *= -1;
            smallest.x *= -1;
            return smallest * overlap;
        }
        private static float GetOverlap(SATProjection p1, SATProjection p2) => MathF.Min(p1.max, p2.max) - MathF.Max(p1.min, p2.min);
        private static SATProjection Project(Polygon polygon, Vec2 axis)
        {
            SATProjection projection = new SATProjection();
            projection.normal = axis;
            projection.min = Vec2.Dot(polygon.vertices[0], axis);
            projection.max = projection.min;

            for (int i = 1; i < polygon.vertices.Length; i++)
            {
                float p = Vec2.Dot(polygon.vertices[i], axis);
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