using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    public class Polygon
    {
        [Field]
        public Vector2[] normals = Array.Empty<Vector2>();
        [Field]
        public Vector2 centroid = Vector2.Zero;
        [Field]
        public Vector2[] uv = Array.Empty<Vector2>();
        [Field]
        public Vector2[] vertices = Array.Empty<Vector2>();

        /// <summary>
        /// Each line will point clockwise.
        /// Will have one line that starts and ends in the same spot if there's only one vertex in the polygon
        /// </summary>
        /// <returns></returns>
        [Method]        
        private void CalculateUV()
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
        [Method]
        private void CalculateNormals()
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
        [Method]
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
        public static BoundingBox2D GetBoundingBox(Vector2[] vertices)
        {
            BoundingBox2D boundingBox = new(vertices[0], vertices[0]);
            foreach (Vector2 vertex in vertices)
                boundingBox.ExpandTo(vertex);
            return boundingBox;
        }
        public void GetBoundingBox(ref BoundingBox2D boundingBox)
        {
            var vertLength = vertices.Length;
            boundingBox.min = vertices[0];
            boundingBox.max = vertices[0];
            for(int i = 1; i < vertLength; i++)
                boundingBox.ExpandTo(vertices[i]);
        }

        /// <summary>
        /// Expects vertices to be structed clockwise
        /// </summary>
        /// <param name="vertices"></param>
        public Polygon() { }
        public Polygon(Vector2[] vertices)
        {
            int vertCount = vertices.Length;
            CheckWindingOrder(vertices);
            this.vertices = vertices;

            //calc normals and centroid
            CalculateNormals();

            //calc uvs (simple)
            CalculateUV();
        }
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

        public static Polygon Rectangle(Vector2 size)
        {
            var vertices = new Vector2[]
            {
                new Vector2(-0.5f, -0.5f),
                new Vector2( 0.5f, -0.5f),
                new Vector2( 0.5f,  0.5f),
                new Vector2(-0.5f,  0.5f),
            };

            return new Polygon(vertices).Transformed(Matrix3x2.CreateScale(size));
        }
        public static Polygon Triangle(float width, float height, float topPosScale = 0.5f)
        {
            Vector2 top = new(topPosScale * width, 0);
            Vector2 right = new(0, height);
            Vector2 left = new(width, height); 

            return new(new Vector2[] { top, right, left});
        }

        public void MoveVertex(int index, Vector2 moveTo)
        {
            vertices[index] = moveTo;
            CalculateNormals();
            CalculateUV();
        }
        public void InsertVertex(int index, Vector2 vertex)
        {
            List<Vector2> verts = vertices.ToList();
            verts.Insert(index, vertex);
            vertices = verts.ToArray();
            CalculateNormals();
            CalculateUV();
        }
        public void RemoveVertexAt(int index)
        {
            List<Vector2> verts = vertices.ToList();
            verts.RemoveAt(index);
            vertices = verts.ToArray();
            CalculateNormals();
            CalculateUV();
        }

        public static Polygon Circle(float radius, int subdivisions)
        {
            var curve = Curve.Circlular(1, subdivisions, radius, false);
            List<Vector2> verts = new();

            for (int i = 0; i < subdivisions; i++)
                verts.Add(curve.Next());

            return new(verts.ToArray());
        }
        public Polygon Transformed(Matrix3x2 matrix)
        {
            Polygon polygon = new(this);
            int vertCount = polygon.vertices.Length;
            for (int i = 0; i < vertCount; i++)
                polygon.vertices[i].Transform(matrix);
            CalculateNormals();
            return polygon;
        }
    }

}

