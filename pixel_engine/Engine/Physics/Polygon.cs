using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    public class Polygon
    {
        public Vector2[] normals = Array.Empty<Vector2>();
        public Vector2 centroid = Vector2.Zero;
        public Vector2[] uv = Array.Empty<Vector2>();
        public Vector2[] vertices = Array.Empty<Vector2>();
        /// <summary>
        /// Each line will point clockwise.
        /// Will have one line that starts and ends in the same spot if there's only one vertex in the polygon
        /// </summary>
        /// <returns></returns>
        public List<Line> GetLines()
        {
            List<Line> lines = new();
            int vertCount = vertices.Length;
            for (int i = 0; i < vertCount; i++)
            {
                var vert1 = vertices[i];
                var vert2 = vertices[(i + 1) % vertCount];
                lines.Add(new(vert1, vert2));
            }
            return lines;
        }

        /// <summary>
        /// Expects vertices to be structed clockwise
        /// </summary>
        /// <param name="vertices"></param>
        public Polygon(Vector2[] vertices)
        {
            this.vertices = vertices;
            int vertCount = vertices.Length;

            //calc normals and centroid
            normals = new Vector2[vertCount];
            centroid = Vector2.Zero;
            for (int i = 0; i < vertCount; i++)
            {
                var vert1 = vertices[i];
                var vert2 = vertices[(i + 1) % vertCount];
                normals[i] = (vert2 - vert1).Normalized();
                centroid += vert1;
            }
            centroid /= vertCount;

            //calc uvs (simple)
            uv = new Vector2[vertCount];
            BoundingBox2D uvBox = GetBoundingBox(vertices);
            Vector2 bbSize = uvBox.max - uvBox.min - Vector2.One;
            if (bbSize.X == 0 || bbSize.Y == 0)
                return;
            for (int i = 0; i < vertCount; i++)
            {
                uv[i] = (vertices[i] - uvBox.min) / bbSize;
            }
        }

        public static BoundingBox2D GetBoundingBox(Vector2[] vertices)
        {
            BoundingBox2D uvBox = new(vertices[0], vertices[0]);
            foreach (Vector2 v in vertices)
                uvBox.ExpandTo(v);
            return uvBox;
        }

        public Polygon() { }
        public Polygon(Polygon polygon)
        {
            vertices = new Vector2[polygon.vertices.Length];
            normals = new Vector2[polygon.normals.Length];
            uv = new Vector2[polygon.uv.Length];
            centroid = polygon.centroid;
            Array.Copy(polygon.vertices,vertices,vertices.Length);
            Array.Copy(polygon.normals, normals, normals.Length);
            Array.Copy(polygon.uv, uv, uv.Length);
        }
        public Polygon OffsetBy(Vector2 offset)
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
            return new(new Vector2[] { new(0, 0), new(width, 0), new(width, height), new(0, height) });
        }
        public static Polygon Triangle(float width, float height, float topPosScale = 0.5f)
        {
            Vector2 top = new(topPosScale * width, 0);
            Vector2 right = new(0, height);
            Vector2 left = new(width, height); 

            return new(new Vector2[] { top, right, left});
        }

    }

}

