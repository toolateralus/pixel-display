using System;
using System.Data;
using System.Numerics;

namespace pixel_renderer
{
    [Serializable]
    public class Joint : Component
    {
        public Node A;
        public Node B;
        internal Vector2? offset;

         string NameA => node.Name;
        [Field] string NameB = "Player";

        [Method]
        public void AttachBodiesByName()
        {
            A = null;
            B = null;

            if (Runtime.Current.GetStage() is Stage stage)
            {
                var a = node; 
                var b = stage.FindNode(NameB);
                if (b is null)
                {
                    Runtime.Log($"Body B of Joint on {node.Name} was not set. No connection was made.");
                    return;
                }
                Runtime.Log($" node {b.Name} Connected to node {a.Name}.");
                
                offset = b.Position - a.Position; 

                if (a.rb != null && b.rb != null)
                {
                    b.RemoveComponent(b.rb);
                    A = a;
                    B = b;
                    A.OnCollided += (c) => OnBodyCollided(c, 0);
                    B.OnCollided += (c) => OnBodyCollided(c, 1);
                }
            }
        }

        private void OnBodyCollided(Collision obj, int body)
        {
            switch (body)
            {
                case 0: 
                    break;

                case 1:
                    // somehow make A body respond to B Body collisions, also this only gets called when B has a rigidbody, which is the opposite of what's expected..
                    break;
            }
        }

      
        public override void FixedUpdate(float delta)
        {
            if (A is not null && B is not null && offset.HasValue)
                B.Position = A.Position + offset.Value;

        }
      
    }
}