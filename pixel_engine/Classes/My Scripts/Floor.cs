using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    /// <summary>
    /// temporary script to keep the floor in place while there is no Kinematic Body (non rigidbodies cannot participate in collision)
    /// </summary>
    internal class Floor : Component
    {
        Vec2 startPos = new(2, Constants.ScreenWidth - 4);
        public override void Update()
        {
           parentNode.position = startPos; 
        }
        public override void OnCollision(Rigidbody collider)
        {
            GetComponent<Sprite>().Randomize(); 
        }
    }
}
