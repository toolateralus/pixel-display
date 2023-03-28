using System.Numerics;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace pixel_renderer
{
    public class Softbody : Component
    {
        [Field] private float minColliderScale = 0.1f;
        [Field] private float maxColliderScale = 1.5f;

        private Collider collider;
        private Polygon model;
        private Rigidbody rb;
        private Collision? lastCollision;
       
        [Field] private int solverIterations = 8;
        [Field] private float deformationRadius = 0.3f;
        [Field] private bool shouldResolve = true;
       
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

                if (lastCollision is not null)
                    lastCollision = null;

                ResolveDeformities();
            }
        }
        public override void OnCollision(Collision col)
        {
            Deformation(col);
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
            if (collider == null || model == null)
                return;
            Polygon poly = collider.GetModel();

            for (int index = 0; index < model.vertices.Length; index++)
            {
                Vector2 vertex = poly.vertices[index];
                Vector2 modelVertex = model.vertices[index];

                bool withinRange;
                Vector2 deformationPos;

                if (WithinDeformationRange(vertex, modelVertex))
                {
                    (Vector2 start, Vector2 end) edge = poly.GetNearestEdge(vertex);

                    float distance = Vector2.Distance(modelVertex, edge.start);
                    float force = Math.Clamp(1f - distance / deformationRadius, 0f, 1f);

                    var edgeVector = edge.end - edge.start;
                    var normal = new Vector2(-edgeVector.Y, edgeVector.X);

                    Vector2 relativeVelocity = modelVertex - col.contact;
                    Vector2 tangent = Vector2.Dot(relativeVelocity, col.normal) * col.normal;
                    Vector2 forceVector = tangent * (-force * Vector2.Dot(modelVertex - edge.start, normal));

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
                Vector2 antiForce = -((currVert - origVert) / solverIterations);
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
