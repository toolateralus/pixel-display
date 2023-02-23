using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    public class Polygon
    {
        public Vec2[] normals = Array.Empty<Vec2>();
        public Vec2 centroid = Vec2.zero;
        public Vec2[] uv = Array.Empty<Vec2>();
        public Vec2[] vertices = Array.Empty<Vec2>();

        public static float LightRadius { get; private set; }

        /// <summary>
        /// Expects vertices to be structed clockwise
        /// </summary>
        /// <param name="vertices"></param>
        public Polygon(Vec2[] vertices)
        {
            this.vertices = vertices;
            int vertCount = vertices.Length;

            //calc normals and centroid
            normals = new Vec2[vertCount];
            centroid = Vec2.zero;
            for (int i = 0; i < vertCount; i++)
            {
                var vert1 = vertices[i];
                var vert2 = vertices[(i + 1) % vertCount];
                normals[i] = (vert2 - vert1).Normal_LHS.Normalized();
                centroid += vert1;
            }
            centroid /= vertCount;

            //calc uvs (simple)
            uv = new Vec2[vertCount];
            BoundingBox2D uvBox = GetBoundingBox(vertices);
            Vec2 bbSize = uvBox.max - uvBox.min - Vec2.one;
            if (bbSize.x == 0 || bbSize.y == 0)
                return;
            for (int i = 0; i < vertCount; i++)
            {
                uv[i] = (vertices[i] - uvBox.min) / bbSize;
            }
        }

        public static BoundingBox2D GetBoundingBox(Vec2[] vertices)
        {
            BoundingBox2D uvBox = new(vertices[0], vertices[0]);
            foreach (Vec2 v in vertices)
                uvBox.ExpandTo(v);
            return uvBox;
        }

        public Polygon() { }
        public Polygon(Polygon polygon)
        {
            vertices = new Vec2[polygon.vertices.Length];
            normals = new Vec2[polygon.normals.Length];
            uv = new Vec2[polygon.uv.Length];
            centroid = new Vec2(polygon.centroid);
            Array.Copy(polygon.vertices,vertices,vertices.Length);
            Array.Copy(polygon.normals, normals, normals.Length);
            Array.Copy(polygon.uv, uv, uv.Length);
        }
        public Polygon OffsetBy(Vec2 offset)
        {
            Polygon polygon = new(this);
            int vertCount = polygon.vertices.Length;
            for (int i = 0; i < vertCount; i++)
                polygon.vertices[i] += offset;
            polygon.centroid += offset;
            return polygon;
        }
        public static Polygon Rectangle(float width, float height)
        {
            return new(new Vec2[] { new(0, 0), new(width, 0), new(width, height), new(0, height) });
        }
        public static Polygon Triangle(float width, float height, float topPosScale = 0.5f)
        {
            Vec2 top = new(topPosScale * width, 0);
            Vec2 right = new(0, height);
            Vec2 left = new(width, height); 

            return new(new Vec2[] { top, right, left});
        }

        public static bool PointInPolygon(Vec2 point, Vec2[] vertices)
        {
            int i, j = vertices.Length - 1;
            bool c = false;
            for (i = 0; i < vertices.Length; i++)
            {
                if (((vertices[i].y > point.y) != (vertices[j].y > point.y)) &&
                    (point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x))
                {
                    c = !c;
                }
                j = i;
            }
            return c;
        }
        public static float GetClosestEdgeDistance(Vec2 point, Vec2[] vertices, out Vec2 v0, out Vec2 v1)
        {
            float minDistance = float.MaxValue;
            v0 = v1 = Vec2.zero;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vec2 p0 = vertices[i];
                Vec2 p1 = vertices[(i + 1) % vertices.Length];

                Vec2 closestPoint = GetClosestPointOnLineSegment(point, p0, p1);

                float distance = (closestPoint - point).Length();
                if (distance < minDistance)
                {
                    minDistance = distance;
                    v0 = p0;
                    v1 = p1;
                }
            }
            return minDistance;
        }
        public static Vec2 GetClosestPointOnLineSegment(Vec2 point, Vec2 p0, Vec2 p1)
        {
            Vec2 dir = p1 - p0;
            float t = Vec2.Dot(point - p0, dir) / dir.SqrMagnitude();
            t = Math.Clamp(t,0,1);
            return p0 + dir * t;
        }
        public Vec2 GetClosestPoint(Vec2 point)
        {
            Vec2 closestPoint = Vec2.zero;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;
                Vec2 vertex = vertices[i];
                Vec2 nextVertex = vertices[j];

                Vec2 edge = nextVertex - vertex;
                Vec2 pointOnEdge = vertex + Vec2.ClampMagnitude(point - vertex, edge.Length()) * edge.Normalized();

                float distance = Vec2.Distance(point, pointOnEdge);

                if (distance < closestDistance)
                {
                    closestPoint = pointOnEdge;
                    closestDistance = distance;
                }
            }

            return closestPoint;
        }
    }

}

