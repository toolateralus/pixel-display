using System.Numerics;
using System.Collections.Generic;
using System;
using System.Security.Cryptography.Xml;

namespace pixel_renderer
{
    public class Softbody : Component
    {
        [Field] private float minColliderScale = 0.1f;
        [Field] private float maxColliderScale = 5f;

        private Collider collider;
        private Polygon model;
        private Rigidbody rb;
        private Collision? lastCollision;
       
        [Field] private int solverIterations = 16;
        [Field] private float deformationRadius = 0.5f;
        [Field] private bool shouldResolve;

        public override void FixedUpdate(float delta)
        {
            if (collider is null) return;

            if(shouldResolve)
                Resolve();

            if (lastCollision is not null)
                lastCollision = null;
        }

        public override void Awake()
        {
            if (rb is null)
                if (!node.TryGetComponent(out rb))
                    return;

            if (collider is null)
            {
                node.TryGetComponent(out collider);
                model = collider.GetModel();
                collider.drawCollider = true;
                collider.drawNormals = true;
                return;
            }
        }

        public override void OnCollision(Collision col)
        {
            Deform1(col);
        }

      
        private void Deform1(Collision col)
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

                withinRange = WithinDeformationRange(vertex, modelVertex);

                if (withinRange)
                {
                    // Get the nearest edge of the collider to the vertex
                    var edge = poly.GetNearestEdge(vertex);

                    // Calculate the distance between the vertex and the collider edge
                    float distance = Vector2.Distance(modelVertex, edge.start);

                    // Calculate the force based on the distance
                    float force = Math.Clamp(1f - distance / deformationRadius, 0f, 1f);

                    // Calculate the normal to the edge
                    var edgeVector = edge.end - edge.start;
                    var normal = new Vector2(-edgeVector.Y, edgeVector.X);

                    // Calculate the relative velocity of the model vertex with respect to the collision normal
                    Vector2 relativeVelocity = modelVertex - col.contact;
                    Vector2 tangent = Vector2.Dot(relativeVelocity, col.normal) * col.normal;

                    // Calculate the force to be applied based on the perpendicular component of the relative velocity
                    Vector2 forceVector = tangent * (-force * Vector2.Dot(modelVertex - edge.start, normal));

                    poly.vertices[index] += forceVector;
                }

            }
            poly.CalculateNormals();
            collider.SetModel(poly);
        }
        private float Force(Collision col, int index)
        {
            // Get the nearest edge of the collider to the vertex
            var edge = col.collider.GetModel().GetNearestEdge(model.vertices[index]);

            // Calculate the distance between the vertex and the collider edge
            float distance = Vector2.Distance(model.vertices[index], edge.start);

            // Calculate the force based on the distance
            float force = Math.Clamp(1f - distance / deformationRadius, 0f, 1f);

            // calcualte normal
            var edgeVector = edge.end - edge.start;
            var normal = new Vector2(-edgeVector.Y, edgeVector.X);

            // Return the negative of the relative velocity as the force
            return -force * Vector2.Dot(model.vertices[index] - edge.start, normal);
        }
        private void Resolve()
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

        internal void Outward(int direction = 1)
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

                if(direction == 1)
                    vert += dir * amt / iterations;
                if (direction == -1)
                    vert -= dir * amt / iterations;

                poly.vertices[index] = vert;
            }
            poly.CalculateNormals(); 
            collider.SetModel(poly);
        }
    }
}
