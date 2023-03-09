using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
    public class SATCollision
    {
        /// <summary>
        /// Finds the smallest vector that will move A out of B
        /// </summary>
        /// <param name="polygonA"></param>
        /// <param name="polygonB"></param>
        /// <returns>The direction to move A out of B, and the amount it must move.</returns>
        public static (Vector2 normal, float depth) GetCollisionData(Polygon polygonA, Polygon polygonB, Vector2 aRelativeVelocity)
        {
            if (polygonA.vertices.Length == 0 || polygonB.vertices.Length == 0)
                return (Vector2.Zero, 0f);

            float overlap = float.MaxValue;
            Vector2 smallest = Vector2.Zero;

            foreach (Vector2 normal in polygonA.normals)
            {
                SATProjection p1 = Project(polygonA, normal);
                SATProjection p2 = Project(polygonB, normal);

                if (!Overlap(p1, p2))
                    return (Vector2.Zero, 0f);

                float o = GetOverlap(p1, p2);
                if (o < overlap)
                {
                    if (aRelativeVelocity.X > 0 &&
                        aRelativeVelocity.Y > 0 &&
                        Vector2.Dot(normal, aRelativeVelocity) < 0)
                        continue;
                    overlap = o;
                    smallest = normal;
                }
            }

            foreach (Vector2 normal in polygonB.normals)
            {
                SATProjection p1 = Project(polygonA, normal);
                SATProjection p2 = Project(polygonB, normal);

                if (!Overlap(p1, p2))
                    return (Vector2.Zero, 0f);

                float o = GetOverlap(p1, p2);
                if (o < overlap)
                {
                    if (aRelativeVelocity.X > 0 &&
                        aRelativeVelocity.Y > 0 &&
                        Vector2.Dot(normal, aRelativeVelocity) > 0)
                        continue;
                    overlap = o;
                    smallest = normal;
                }
            }

            if (smallest == Vector2.Zero)
                return (Vector2.Zero, 0f);

            Vector2 bToAOffset = polygonA.centroid - polygonB.centroid;
            if (Vector2.Dot(bToAOffset, smallest) < 0)
                smallest *= -1;

            return (smallest, overlap);
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