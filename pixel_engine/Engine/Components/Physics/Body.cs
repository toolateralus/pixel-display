using System;
using System.Numerics;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace pixel_renderer
{
    public class Softbody : Component
    {
        [Field] private float minScale = 0.25f;
        [Field] private float maxScale = 2f;
        [Field] private Collider? collider = null;
                private Curve curve;
                private Polygon model;
                private Rigidbody rb;

        List<Vector2> activeForces;

        public override void Awake()
        {
            if (rb is null)
                if (!node.TryGetComponent(out rb))
                    return;


            curve ??= Curve.Linear();

            if (collider is null)
            {
                node?.TryGetComponent(out collider);
                model = collider?.model;
                collider.drawCollider = true;
                collider.drawNormals = true;
                return;
            }
            model ??= collider.model;
        }
        public override void FixedUpdate(float delta)
        {
            if (!collisionThisFrame && activeForces != null && activeForces.Count  == model.vertices.Length)
            {
                Resolve(activeForces);
            }

            collisionThisFrame = false;
        }

        public override void OnCollision(Collision col)
        {
            collisionThisFrame = true; 
            Deform(col);
        }
        Vector2 min = new Vector2( -1f, -1f);
        
        Vector2 max = new Vector2( 1f, 1f);
        
        private bool collisionThisFrame;

        private int solverIterations = 6;

        private void Deform(Collision col)
        {
            if (collider is null || model is null)
                return;

            if (activeForces is null)
            {
                activeForces = new(model.vertices.Length);
            }

            if (activeForces.Count != model.vertices.Length)
            {
                Resolve(activeForces);
                activeForces = new(model.vertices.Length);
            }

            Polygon poly = new(collider.model);

            for (int index = 0; index < poly.vertices.Length; index++)
            {
                var (within, pos) = WithinDeformationRange(poly.vertices[index], model.vertices[index]);

                if (within)
                {
                    var force = Force(poly, index);
                    poly.vertices[index] += force;

                    if (activeForces.Count > index)
                        activeForces[index] += force;
                    else activeForces.Add(force);

                    continue;
                }
                poly.vertices[index] = pos;
            }
            collider.model = poly;
            collider.model.CalculateNormals();
        }

        private void Resolve(List<Vector2> activeForces)
        {
            if (activeForces.Count == 0)
                return; 

            // our modded model
            Polygon poly = new(collider.model);

                for (int x = 0; x < model.vertices.Length; x++)
                {
                    Vector2 origVert = model.vertices[x];
                    for (int y = 0; y < poly.vertices.Length; y++)
                    {

                        if (y > activeForces.Count)
                            return; 

                        Vector2 vert = poly.vertices[y];

                        if (vert - activeForces[y] == origVert)
                            continue;

                        Vector2 antiForce = -(activeForces[y] / (float)solverIterations);
                     
                        Vector2 difference = (poly.vertices[y] + antiForce) - model.vertices[y];

                        float dot = Vector2.Dot(antiForce, difference); 
                        
                        if (dot > 0)
                        {
                            poly.vertices[y] = model.vertices[y];
                            continue; 
                        }



                        poly.vertices[y] += antiForce;
                        
                        activeForces[y] += antiForce;
                        

                        

                        if (activeForces[y] == Vector2.Zero)
                            activeForces.RemoveAt(y);
                    }
                }
            collider.model = poly;
            collider.model.CalculateNormals(); 
        }

        private Vector2 Force(Polygon poly, int index)
        {
            return Vector2.Clamp(curve.Next() * -rb.velocity, min, max);
        }

        private (bool within, Vector2 result) WithinDeformationRange(Vector2 vert, Vector2 original)
        {
            float scale = vert.Length() / original.Length();
            var min = minScale;
            var max = maxScale;
            if (scale < min)
                return (false, original * min);
            if (scale > max)
                return (false, original * max);
            return (true, original * scale);
        }
        public static Node SoftBody()
        {
            Node node = Rigidbody.Standard();


            if(!node.TryGetComponent(out Collider col))
                return node;

            var sb = node.AddComponent<Softbody>();
            sb.collider = col;
            return node; 
        }
    }
}
