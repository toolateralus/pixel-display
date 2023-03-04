using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    public class Light : Component
    {
        [Field] public float brightness = .5f;
        [Field] public Vector2 startPos = Vector2.One;
        [Field] public float radius = 55;
        [Field] public Pixel color = ExtensionMethods.Lerp(Color.White, Color.Yellow, 0.7f);
       
        public override void OnDrawShapes()
        {
            float sliceAngle = 360f / 6; // divide circle into 6 equal slices
            float radius = 5f; // radius of the circle
            var center = node.Position + Vector2.One * radius / 2;

            for (int i = 0; i < 6; i++)
            {
                // calculate start and end points of the line
                float startAngle = i * sliceAngle;
                float endAngle = (i + 1) * sliceAngle;
                Vector2 startPt = center + new Vector2(MathF.Cos(startAngle * CMath.PI / 180) * radius, (float)(Math.Sin(startAngle * CMath.PI / 180) * radius));
                Vector2 endPt = startPt + new Vector2(MathF.Cos(endAngle * CMath.PI / 180) * radius, (float)(Math.Sin(endAngle *  CMath.PI / 180) * radius));

                ShapeDrawer.DrawLine(startPt, endPt, Color.Yellow);
                ShapeDrawer.DrawLine(endPt, endPt + center);
            }
        }
        public static Node Standard()
        {
            Node x = new("Light");
            x.AddComponent<Light>();
            return x; 
        }
    }
}
