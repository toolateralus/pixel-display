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
        Rigidbody rb = new();
        Vec2 startPos = new(); 
        public override void Awake()
        {
            base.Awake();
            startPos = parentNode.position; 
            rb = parentNode.GetComponent<Rigidbody>();
        }

        public override void FixedUpdate(float delta)
        {
            rb.velocity = CMath.Negate(Constants.TerminalVec2());
            rb.parentNode.position = startPos; 
        }
    }
}
