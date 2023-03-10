using System;
using System.Numerics;
using System.Windows.Media.Media3D;
using System.Xml;

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

        public override void OnCollision(Collision col)
        {
            Deform(col);
        }
        Vector2 min = new Vector2( -0.25f, -0.25f);
        Vector2 max = new Vector2( 0.25f, 0.25f);
        private void Deform(Collision col)
        {
            if (collider is null || model is null)
                return;

            Polygon poly = new(collider.model);

            for (int index = 0; index < poly.vertices.Length; index++)
            {
                var (within, pos) = WithinDeformationRange(poly.vertices[index], model.vertices[index]);

                if (within)
                {
                    poly.vertices[index] += Vector2.Clamp(curve.Next() * -rb.velocity, min, max);
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
            Node node = Rigidbody.Standard();


            if(!node.TryGetComponent(out Collider col))
                return node;

            var sb = node.AddComponent<Softbody>();
            sb.collider = col;
            return node; 
        }
    }
}
