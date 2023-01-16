using pixel_renderer;
using System;
using Polygon = pixel_renderer.Polygon;

public class SATCollision
{
    public static Vec2 GetMinimumDepthVector(Polygon polygonA, Polygon polygonB)
    {
        float overlap = float.MaxValue;
        Vec2 smallest = Vec2.zero;

        for (int i = 0; i < polygonA.normals.Length; i++)
        {
            SATProjection p1 = Project(polygonA, polygonA.normals[i]);
            SATProjection p2 = Project(polygonB, polygonA.normals[i]);

            if (!Overlap(p1, p2))
            {
                return Vec2.zero;
            }
            else
            {
                float o = GetOverlap(p1, p2);
                if (o < overlap)
                {
                    overlap = o;
                    smallest = polygonA.normals[i];
                    if (Vec2.Dot(polygonB.centroid - polygonA.centroid, smallest) < 0)
                    {
                        smallest = CMath.Negate(smallest);
                    }
                }
            }
        }

        for (int i = 0; i < polygonB.normals.Length; i++)
        {
            SATProjection p1 = Project(polygonA, polygonB.normals[i]);
            SATProjection p2 = Project(polygonB, polygonB.normals[i]);

            if (!Overlap(p1, p2))
            {
                return Vec2.zero;
            }
            else
            {
                float o = GetOverlap(p1, p2);
                if (o < overlap)
                {
                    overlap = o;
                    smallest = polygonB.normals[i];
                    if (Vec2.Dot(polygonA.centroid - polygonB.centroid, smallest) < 0)
                    {
                        smallest = CMath.Negate(smallest);
                    }
                }
            }
        }

        return smallest * overlap;
    }
    private static float GetOverlap(SATProjection p1, SATProjection p2)
    {
        return MathF.Min(p1.max, p2.max) - MathF.Max(p1.min, p2.min);
    }
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
