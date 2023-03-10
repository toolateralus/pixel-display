using System;
using System.Numerics;
using System.Windows.Media.Media3D;
using System.Xml;

namespace pixel_renderer
{
    public class Softbody : Rigidbody
    {
        [Field] private float minScale = 0.25f;
        [Field] private float maxScale = 2f;
        [Field] private Collider? collider = null;
                private Curve curve;
                private Polygon model;

        public override void Update()
        {
            if (curve is null)
            {
                curve = Curve.Circlular(1, 6, .001f, true);
            }

            if (collider is null)
            {
                node?.TryGetComponent(out collider);
                model = collider?.model;
                return; 
            }

            collider.drawCollider = true;
            collider.drawNormals = true;
        }
        
        public override void OnTrigger(Collision col)
        {
            if (this.collider is null || model is null)
                return;

            Polygon poly = new(model.vertices);

            for (int index = 0; index < poly.vertices.Length; index++)
            {
                var (within, pos) = WithinDeformationRange(poly.vertices[index], model.vertices[index]);
                if (within)
                {
                    float distance = Vector2.Distance(poly.vertices[index], collider.model.centroid);
                    
                    Vector2 direction = (poly.vertices[index] - collider.Polygon.centroid).Normalized();

                    float vel = (velocity.Length() / 2);

                    poly.vertices[index] += curve.Next() * direction; 

                    continue;
                }
                poly.vertices[index] = pos;
            }
            collider.model = poly;
            collider.model.CalculateNormals(); 
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
            Node node = Standard();
            if(!node.TryGetComponent(out Rigidbody rb)) 
                return node;

            node.RemoveComponent(rb);

            if(!node.TryGetComponent(out Collider col))
                return node;

            var sb = node.AddComponent<Softbody>();
            sb.collider = col;
            return node; 
        }
    }
}
