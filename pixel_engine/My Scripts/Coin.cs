using Pixel;
using Pixel.Types.Components;
using Pixel.Types.Physics;

namespace Pixel_Engine.My_Scripts
{
    internal class Coin : Component
    {
        public static int Count; 
        public override void Dispose()
        {
            
        }
        internal static Node Standard()
        {
            var node = Rigidbody.StaticBody();
            var col =  node.GetComponent<Collider>();
            var spr = node.GetComponent<Sprite>();
            
            spr.color = Color.White; 
            col.IsTrigger = true;
            
            var coin = node.AddComponent<Coin>();
            node.OnTriggered += OnCollected;

            return node;
        }
        static void OnCollected(Collision col)
        {
            Count += 1;
        }
    }
}