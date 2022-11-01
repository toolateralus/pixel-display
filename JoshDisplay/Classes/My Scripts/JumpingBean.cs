using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelRenderer.Components; 
namespace PixelRenderer
{
    internal class JumpingBean : Component
    {
        Rigidbody rb; 
        int i = 0; 
        public override void Awake()
        {
            rb = parentNode.GetComponent<Rigidbody>();
        }
        public override void FixedUpdate()
        {
            if (i < 250)
            {
                i++;
                rb.velocity += WaveForms.GetPointOnSine(); 
            }
            else if( i > -125)
            {
                i--;
                rb.velocity -= WaveForms.GetPointOnSine() * 4; 
            }
        }
    }
}
