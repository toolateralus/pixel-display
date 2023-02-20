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
            BoundingBox2D uvBox = new(vertices[0], vertices[0]);
            foreach (Vec2 v in vertices)
            {
                uvBox.ExpandTo(v);
            }
            Vec2 bbSize = uvBox.max - uvBox.min - Vec2.one;
            for (int i = 0; i < vertCount; i++)
            {
                uv[i] = (vertices[i] - uvBox.min) / bbSize;
            }
        }
    }
    
}

