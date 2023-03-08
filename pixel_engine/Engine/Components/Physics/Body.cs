using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class Softbody : Rigidbody
    {
        [Field] Collider? collider = null;

        [Field] float minDistFromCenter = 0.4f;
        [Field] float maxDistFromCenter = 1.5f;
        private Polygon model;

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


            List<Vector2> forces = new();

            for (int index = 0; index < poly.vertices.Length; index++)
            {
                Vector2 vert = poly.vertices[index];
                var (within, distance) = WithinDeformationRange(Vector2.Zero, vert);
                if (within) DeformVertex(poly, index, distance, velocity);
                else CorrectVertex(poly, index, vert, distance);
            }
        }

        private void DeformVertex(Polygon poly, int index, float distance, Vector2 velocity)
        {
            int ct = poly.vertices.Length;
            if (ct < index)
                return;
            if (distance > 0)
                poly.vertices[index] += velocity;
            else poly.vertices[index] -= velocity;
        }

        private (bool within, float distance) WithinDeformationRange(Vector2 pos, Vector2 vert)
        {
            float distance = Vector2.Distance(pos, vert);
            
            var min = minDistFromCenter;
            var max = maxDistFromCenter;
            
            if (distance < min)
                return (false, min - distance);
            
            if (distance > max)
                return (false, max - distance);

            return (true, distance);
        }

        private static void CorrectVertex(Polygon poly, int i, Vector2 vert, float difference)
        {
            poly.vertices[i] = new(vert.X - difference, vert.Y - difference);
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
