using System;

namespace pixel_renderer
{
    public class Polygon
    {
        public Vec2[] normals;
        public Vec2 centroid;
        public Vec2[] uv;
        public Vec2[] vertices;
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

        public Polygon()
        {
            normals = Array.Empty<Vec2>();
            centroid = Vec2.zero;
            uv = Array.Empty<Vec2>();
            vertices = Array.Empty<Vec2>();
        }
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
        public static Polygon Triangle(float length)
        {
            Vec2 zero = new Vec2(0,length);
            Vec2 one = new Vec2(0.5f * length, length);
            Vec2 two = new Vec2(length, length);

            return new(new Vec2[] { zero, one, two});
        }
    }
    
}

