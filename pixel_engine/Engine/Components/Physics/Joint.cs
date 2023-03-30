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
                else if (ThisBodyIs_A) 
                    a = node; 

                if (b is null)
                {
                    Runtime.Log($"Body B of Joint on {node.Name} was not set. No connection was made.");
                    return;
                }
                Runtime.Log($" node {B.Name} Connected to node {A.Name}.");
                A = a;
                B = b; 
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