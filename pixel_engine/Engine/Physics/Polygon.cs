using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Policy;

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
            int vertCount = vertices.Length;
            CheckWindingOrder(vertices);
            this.vertices = vertices;

            //calc normals and centroid
            RecalculateNormals();

            //calc uvs (simple)
            RecalculateUVs();
        }

        private void RecalculateUVs()
        {
            int vertCount = vertices.Length;
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

        private void RecalculateNormals()
        {
            int vertCount = vertices.Length;
            normals = new Vector2[vertCount];
            centroid = Vector2.Zero;
            for (int i = 0; i < vertCount; i++)
            {
                var vert1 = vertices[i];
                var vert2 = vertices[(i + 1) % vertCount];
                normals[i] = (vert2 - vert1).Normal_LHS();
                centroid += vert1;
            }
            centroid /= vertCount;
        }

        private static void CheckWindingOrder(Vector2[] vertices)
        {
            float area = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;
                area += vertices[i].X * vertices[j].Y - vertices[j].X * vertices[i].Y;
            }

            if (area < 0)
                Array.Reverse(vertices);
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
        public Polygon Transform(Matrix3x2 matrix)
        {
            Polygon polygon = new(this);
            int vertCount = polygon.vertices.Length;
            for (int i = 0; i < vertCount; i++)
                polygon.vertices[i] = Vector2.Transform(polygon.vertices[i], matrix);
            polygon.centroid = Vector2.Transform(polygon.centroid, matrix);
            return polygon;
        }
        public void MoveVertex(int index, Vector2 moveTo)
        {
            vertices[index] = moveTo;
            RecalculateNormals();
            RecalculateUVs();
        }
        public void InsertVertex(int index, Vector2 vertex)
        {
            List<Vector2> verts = vertices.ToList();
            verts.Insert(index, vertex);
            vertices = verts.ToArray();
            RecalculateNormals();
            RecalculateUVs();
        }
        public void RemoveVertexAt(int index)
        {
            List<Vector2> verts = vertices.ToList();
            verts.RemoveAt(index);
            vertices = verts.ToArray();
            RecalculateNormals();
            RecalculateUVs();
        }
        public static Polygon Circle(float radius, int subdivisions)
        {
            var curve = Curve.Circlular(1, subdivisions, radius, false);
            List<Vector2> verts = new();

            for (int i = 0; i < subdivisions; i++)
                verts.Add(curve.Next());

            return new(verts.ToArray());
        }

    }

}

