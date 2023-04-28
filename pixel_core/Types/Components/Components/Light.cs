﻿using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Media.Animation;

namespace Pixel
{
    public class Light : Component
    {
        [Field] public float brightness = 250f;
        [Field] public float radius = 25;
        [Field] public Color color = ExtensionMethods.Lerp(System.Drawing.Color.White, System.Drawing.Color.Yellow, 0.125f);

        public override void Dispose(){}
        public static Node Standard()
        {
            Node x = Rigidbody.StaticBody();
            x.Name = "light";
            x.AddComponent<Light>();
            x.Position = new(0, 1);
            return x;
        }
    }
}
