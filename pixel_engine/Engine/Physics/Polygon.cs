using Newtonsoft.Json;
using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    /// <summary>
    /// note :our Polygon uses a clockwise winding order
    /// </summary>
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
        public Pixel debuggingColor = Pixel.Random;

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
        public void CalculateNormals()
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

        public static Polygon Rectangle(Vector2 size) =>
            UnitSquare().Transformed(Matrix3x2.CreateScale(size));

        public static Polygon UnitSquare() =>
            new(new Vector2[]
            {
                new Vector2(-0.5f, -0.5f),
                new Vector2( 0.5f, -0.5f),
                new Vector2( 0.5f,  0.5f),
                new Vector2(-0.5f,  0.5f),
            });
        public static Polygon Square(float centerToCornerDistance) =>
            new(new Vector2[]
            {
                new Vector2(-centerToCornerDistance, -centerToCornerDistance),
                new Vector2( centerToCornerDistance, -centerToCornerDistance),
                new Vector2( centerToCornerDistance,  centerToCornerDistance),
                new Vector2(-centerToCornerDistance,  centerToCornerDistance),
            });
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
                verts.Add(curve.GetValue(true));

            return new(verts.ToArray());
        }

        public static Polygon nGon(float size, int sides = 6)
        {
            float angle = 2 * MathF.PI / sides;
            List<Vector2> verts = new();
            for (int i = 0; i < sides; i++)
            {
                float x = size * MathF.Cos(i * angle);
                float y = size * MathF.Sin(i * angle);
                verts.Add(new Vector2(x, y));
            }
            return new Polygon(verts.ToArray());
        }

        public Polygon Transformed(Matrix3x2 matrix)
        {
            Polygon polygon = new(this);
            polygon.Transform(matrix);
            return polygon;
        }
        public void Transform(Matrix3x2 matrix)
        {
            int vertCount = vertices.Length;
            for (int i = 0; i < vertCount; i++)
                vertices[i].Transform(matrix);
            CalculateNormals();
        }

        public static Vector2 ClosestPointOnSegment(Vector2 point1, Vector2 point2, Vector2 point)
        {
            Vector2 direction = point2 - point1;
            float lengthSquared = direction.LengthSquared();

            if (lengthSquared == 0)
                return point1;

            float t = Vector2.Dot(point - point1, direction) / lengthSquared;
            t = Math.Clamp(t, 0, 1);

            return point1 + direction * t;
        }

        public (Vector2 start, Vector2 end) GetNearestEdge(Vector2 point)
        {
            if (vertices.Length < 2)
                throw new InvalidOperationException("Polygon must have at least 2 vertices.");

            Vector2 nearestPoint1 = vertices[0];
            Vector2 nearestPoint2 = vertices[1];
            float nearestDistance = Vector2.Distance(point, nearestPoint1);

            for (int i = 1; i < vertices.Length; i++)
            {
                int j = (i + 1) % vertices.Length;

                Vector2 point1 = vertices[i];
                Vector2 point2 = vertices[j];

                Vector2 closestPoint = ClosestPointOnSegment(point1, point2, point);
                float distance = Vector2.Distance(point, closestPoint);

                if (distance < nearestDistance && point1 != point2)
                {
                    nearestPoint1 = point1;
                    nearestPoint2 = point2;
                    nearestDistance = distance;
                }
            }
            return (nearestPoint1, nearestPoint2);
        }
        public void CopyTo(ref Polygon polygon)
        {
            if (polygon.vertices.Length != vertices.Length)
                polygon.vertices = new Vector2[vertices.Length];
           
            if (polygon.normals.Length != normals.Length)
                polygon.normals = new Vector2[normals.Length];

            if (polygon.uv.Length != uv.Length)
                polygon.uv = new Vector2[uv.Length];

            Array.Copy(vertices, polygon.vertices, vertices.Length);
            Array.Copy(normals, polygon.normals, normals.Length);
            Array.Copy(uv, polygon.uv, uv.Length);

            polygon.centroid = centroid;
        }

    }

}

