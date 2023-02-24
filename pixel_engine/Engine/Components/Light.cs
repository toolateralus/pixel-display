using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace pixel_renderer
{
    public class Light : Component
    {
        public float brightness = 100f;
        public Vec2 startPos = Vec2.one;
        public float radius = 24;
        public Color color = Color.FromArgb(255, 235, 255, 245);

        public override void Update()
        {
            parent.Position = startPos;
        }
        public override void OnDrawShapes()
        {
            float sliceAngle = 360f / 6; // divide circle into 6 equal slices
            float radius = 5f; // radius of the circle
            var center = parent.Position + Vec2.one * radius / 2;

            for (int i = 0; i < 6; i++)
            {
                // calculate start and end points of the line
                float startAngle = i * sliceAngle;
                float endAngle = (i + 1) * sliceAngle;
                Vec2 startPt = center + new Vec2(MathF.Cos(startAngle * CMath.PI / 180) * radius, (float)(Math.Sin(startAngle * CMath.PI / 180) * radius));
                Vec2 endPt = startPt + new Vec2(MathF.Cos(endAngle * CMath.PI / 180) * radius, (float)(Math.Sin(endAngle *  CMath.PI / 180) * radius));

                ShapeDrawer.DrawLine(startPt, endPt, Color.Yellow);
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
