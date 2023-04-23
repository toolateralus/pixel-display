using System.Numerics;
using System;

namespace pixel_renderer
{
    public class Softbody : Component
    {
        public override void Dispose()
        {
            collider = null;
            model = null;
            rb = null;
        }
        [Field] private float minColliderScale = 0.1f;
        [Field] private float maxColliderScale = 1.5f;

        private Collider collider;
        private Polygon model;
        private Rigidbody rb;
       
        [Field] private int resolverIterations = 32;
        /// <summary>
        /// the max deformation represented as radius around the vertex
        /// </summary>
        [Field] private float deformationRadius = 0.15F;
        [Field] private bool shouldResolve = true;
        private Collision collision;

        [Method]
        private void BakeOriginalShape()
        {
            TryGetComponent(out collider);
            model = collider.GetModel();
            collider.drawCollider = true;
            collider.drawNormals = true;
        }
        public override void FixedUpdate(float delta)
        {
            if (collision != null)
                Deformation(collision);

            if (shouldResolve)
            {
                if (collider is null)
                {
                    TryGetComponent(out collider);
                    collider.SetModel(Polygon.nGon(2, 8));
                    BakeOriginalShape();
                }
                if (rb is null)
                    TryGetComponent(out rb);

                ResolveDeformities();
            }
            collision = null;
        }
        public override void OnCollision(Collision col)
        {
            collision = col;
        }
        internal void UniformDeformation(int direction = 1)
        {
            if (collider is null|| model is null) return;

            Polygon poly = collider.GetModel();

            const int iterations = 16;
            const float amt = 0.1f;

            for (int i = 0; i < poly.vertices.Length * iterations; i++)
            {
                var index = i / iterations;
                Vector2 dir = poly.centroid - poly.vertices[index];
                dir.Normalize();
                Vector2 vert = poly.vertices[index];

                if (direction == 1)
                    vert += dir * amt / iterations;
                if (direction == -1)
                    vert -= dir * amt / iterations;

                poly.vertices[index] = vert;
            }
            poly.CalculateNormals(); 
            collider.SetModel(poly);
        }
        private void Deformation(Collision col)
        {
            if (collider == null || model == null || collision == null)
                return;

            Polygon poly = collider.GetModel();

            for (int index = 0; index < model.vertices.Length; index++)
            {
                Vector2 vertex = poly.vertices[index];
                Vector2 modelVertex = model.vertices[index];

                bool withinRange = WithinDeformationRange(vertex, modelVertex);
                if (withinRange)
                {
                    (Vector2 start, Vector2 end) edge = poly.GetNearestEdge(vertex);

                    float distance = Vector2.Distance(modelVertex, edge.start);
                    float force = Math.Clamp(2f - distance / deformationRadius, 0.1f, 2f);

                    var edgeVector = edge.end - edge.start;
                    var normal = new Vector2(-edgeVector.Y, edgeVector.X);

                    Vector2 relativeVelocity = modelVertex - col.contact;
                    Vector2 tangent = Vector2.Dot(relativeVelocity, col.normal) * col.normal;
                    float dot = Vector2.Dot(modelVertex - edge.start, normal);
                    Vector2 forceVector = tangent * (force * dot);

                    poly.vertices[index] += forceVector;
                }
            }
            poly.CalculateNormals();
            collider.SetModel(poly);
        }
        private void ResolveDeformities()
        {
            if (collider is null || model is null) 
                return; 
            Polygon poly = collider.GetModel();
            for (int i = 0; i < model.vertices.Length; i++)
            {
                Vector2 origVert = model.vertices[i];
                Vector2 currVert = poly.vertices[i];
                Vector2 antiForce = -((currVert - origVert) / resolverIterations);
                poly.vertices[i] += antiForce;
            }
            poly.CalculateNormals();
            collider.SetModel(poly);
        }
        private bool WithinDeformationRange(Vector2 vert, Vector2 original)
        {
            float scale = vert.Length() / original.Length();
           
            var min = minColliderScale;
            var max = maxColliderScale;

            if (scale < min)
                return false;

            if (scale > max)
                return false;

            return true; 
        }
        public static Node SoftBody()
        {
            Node node = Rigidbody.Standard();


            if(!node.TryGetComponent(out Collider col))
                return node;

            var sb = node.AddComponent<Softbody>();
            return node; 
        }
    }
}
