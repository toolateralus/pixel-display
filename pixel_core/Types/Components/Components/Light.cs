using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Drawing;
using System.Numerics;

namespace Pixel
{
    public class Light : Component
    {
        [Field] public float brightness = 1f;
        [Field] public float radius = 32;
        [Field] public Color color = ExtensionMethods.Lerp(System.Drawing.Color.White, System.Drawing.Color.Yellow, 0.125f);
        public override void Dispose(){}
        public static Node Standard()
        {
            Node x = Rigidbody.Standard();
            x.Name = "light";
            x.AddComponent<Light>();
            x.Position = new(0, 1);
            return x;
        }
    }
}
