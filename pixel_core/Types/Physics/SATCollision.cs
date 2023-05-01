using System;
using System.Collections.Generic;
using System.Numerics;

namespace Pixel.Types.Physics
{
    /// <summary>
    /// A helper class for performing SAT collisions, our supported algorithm.
    /// </summary>
    public class SATCollision
    {
        static Vector2 zero = Vector2.Zero;
        /// <summary>
        /// Finds the smallest vector that will move A out of B
        /// </summary>
        /// <param name="polygonA"></param>
        /// <param name="polygonB"></param>
        /// <returns>The direction to move A out of B, and the amount it must move.</returns>
        public static Collision? GetCollisionData(Polygon polygonA, Polygon polygonB, Vector2 aRelativeVelocity)
        {
            if (polygonA.vertices.Length == 0 || polygonB.vertices.Length == 0)
                return null;

            Collision collision = new();
            collision.depth = float.MaxValue;

            List<Vector2> normals = new(polygonA.normals);
            normals.AddRange(polygonB.normals);

            foreach (Vector2 normal in normals)
            {
                SATProjection projA = Project(polygonA, normal);
                SATProjection projB = Project(polygonB, normal);

                if (!Overlap(projA, projB))
                {
                    collision.normal = zero;
                    return null;
                }

                float overlap = GetOverlap(projA, projB);
                if (overlap < collision.depth)
                {
                    collision.depth = overlap;
                    collision.normal = normal;
                    collision.thisProjection = projA;
                    collision.otherProjection = projB;
                }
            }

            if (collision.normal == zero)
                return null;

            Vector2 offsetA = polygonA.centroid - polygonB.centroid;
            if (Vector2.Dot(offsetA, collision.normal) < 0)
                collision.normal *= -1;

            return collision;
        }
        private static float GetOverlap(SATProjection p1, SATProjection p2) => MathF.Min(p1.max, p2.max) - MathF.Max(p1.min, p2.min);
        private static SATProjection Project(Polygon polygon, Vector2 axis)
        {
            SATProjection projection = new SATProjection();
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
            return p1.min <= p2.max && p1.max >= p2.min;
        }

        private static void ProjectAllNormals(List<Vector2> normals, Polygon polygonA, Polygon polygonB, ref Collision collision)
        {
            foreach (Vector2 normal in normals)
            {
                SATProjection projA = Project(polygonA, normal);
                SATProjection projB = Project(polygonB, normal);

                if (!Overlap(projA, projB))
                {
                    collision.normal = zero;
                    return;
                }

                float overlap = GetOverlap(projA, projB);
                if (overlap < collision.depth)
                {
                    collision.depth = overlap;
                    collision.normal = normal;
                    collision.thisProjection = projA;
                    collision.otherProjection = projB;
                }
            }
        }
    }

}