using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class Softbody : Rigidbody
    {
        [Field] Collider? collider = null;
        [Field] float minScale = 0.25f;
        [Field] float maxScale = 2f;

        private Polygon model = default;
        private Vector2 maxDeformationAmount = new(0.001f, 0.001f);

        public override void Update()
        {
            if (collider is null)
                node?.TryGetComponent(out collider);

            if (collider is null) return; 
            collider.drawCollider = true;
            collider.drawNormals = true;
            model = collider.model;

        }
        public override void OnCollision(Collider col)
        {
            if (this.collider is null)
                return;

            Polygon poly = new(model);

            for (int index = 0; index < poly.vertices.Length; index++)
            {
                var (within, pos) = WithinDeformationRange(poly.vertices[index], model.vertices[index]);
                if (within)
                {
                    float distance = Vector2.Distance(poly.vertices[index], collider.Polygon.centroid);
                    
                    Vector2 deformationAmount = Vector2.Lerp(Vector2.Zero, maxDeformationAmount, distance / (collider.Scale / 2).Length());

                    Vector2 direction = (poly.vertices[index] - collider.Polygon.centroid).Normalized();
                    
                    poly.vertices[index] += direction * deformationAmount;
                    
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
