using System;
using System.Numerics;
using System.Xml;

namespace pixel_renderer
{
    public class Softbody : Rigidbody
    {
        [Field] Collider? collider = null;
        [Field] float minScale = 0.25f;
        [Field] float maxScale = 2f;

        Curve curve;

        private Vector2 maxDeformationAmount = new(0.001f, 0.001f);

        public override void Update()
        {
            usingGravity = false;

            if (curve is null)
            {
                curve = Curve.Circlular(1, 16, 3, true);
            }

            if (collider is null)
            {
                node?.TryGetComponent(out collider);
                return; 
            }

            collider.drawCollider = true;
            collider.drawNormals = true;
            collider.IsTrigger = true;
        }

        
        public override void OnTrigger(Collision col)
        {
            if (this.collider is null)
                return;

            Polygon poly = new(collider.model);

            for (int index = 0; index < poly.vertices.Length; index++)
            {
                var (within, pos) = WithinDeformationRange(poly.vertices[index], collider.model.vertices[index]);
                if (within)
                {
                    float distance = Vector2.Distance(poly.vertices[index], collider.model.centroid);
                    
                    Vector2 deformationAmount = Vector2.Lerp(Vector2.Zero, maxDeformationAmount, distance / (collider.Scale / 2).Length());
                    
                    Vector2 direction = (poly.vertices[index] - collider.Polygon.centroid).Normalized();

                    if (JRandom.Bool())
                        poly.vertices[index] += direction * curve.Next();
                    else poly.vertices[index] -= direction * curve.Next();
                    continue;
                }
                poly.vertices[index] = pos;
            }
            collider.model = poly; 
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
