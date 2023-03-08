using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class Softbody : Rigidbody
    {
        [Field] Collider? collider = null;
        [Field] float colliderDistance = 0f;

        int i = 0;
        public override void Update()
        {
            if (collider is null)
                node?.TryGetComponent(out collider);
        }

        public override void OnCollision(Collider collider)
        {
            if (this.collider is null)
                return;
            colliderDistance = Vector2.Distance(Position, collider.Position);
            Runtime.Log(colliderDistance);
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
