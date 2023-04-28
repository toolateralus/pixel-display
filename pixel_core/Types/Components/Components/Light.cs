using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Drawing;
using System.Numerics;

namespace Pixel
{
    public class Light : Component
    {
        [Field] public float brightness = 0.5f;
        [Field] public float radius = 25;
        [Field] public Color color = ExtensionMethods.Lerp(System.Drawing.Color.White, System.Drawing.Color.Yellow, 0.7f);
        
        public override void Dispose()
        {

        }



        public static Node Standard()
        {
            Node x = new("Light");
            x.AddComponent<Light>();
            return x;
        }
    }
}
