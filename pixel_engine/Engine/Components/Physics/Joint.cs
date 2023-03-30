using System;
using System.Data;
using System.Numerics;

namespace pixel_renderer
{
    [Serializable]
    public class Joint : Component
    {
        [Method]
        public void AttachBodiesByName()
        {
            A = null;
            B = null;

            if (Runtime.Current.GetStage() is Stage stage)
            {
                var a = stage.FindNode(NameA);
                var b = stage.FindNode(NameB);
               

                if (a is null && !ThisBodyIs_A)
                {
                    Runtime.Log($"Body A of Joint on {node.Name} was not set, and it's not in self select mode. No connection was made.");
                    return;
                }
                if (b is null)
                {
                    Runtime.Log($"Body B of Joint on {node.Name} was not set. No connection was made.");
                    return;
                }
                
                if (ThisBodyIs_A)
                    a = node;

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

        Vector2? offset;

        public override void FixedUpdate(float delta)
        {
            if (A is not null && B is not null && offset.HasValue)
                B.Position = A.Position + offset.Value;

        }
        public Node A;
        public Node B;

        [Field] public bool ThisBodyIs_A = true; 
        [Field] string NameA = "";
        [Field] string NameB= "Player";
    }
}