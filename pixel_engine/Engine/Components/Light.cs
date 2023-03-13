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
        Random random = new Random();
        float length = 0;
        Pixel lastColor;
        private float j;

        public override void OnDrawShapes()
        {
            int lineCt = 12; 
            float sliceAngle = 360f / lineCt; // divide circle into 6 equal slices
            float radius = 5f; // radius of the circle
            var center = node.Position;

            Color color = Pixel.White; 

            length += 0.01f;

            if(length > 7)
                length = 0.5f;
            if (length > 1)
                color = Color.White;
            if (length > 2)
                color = Color.Yellow;
            if (length > 3)
                color = Color.Orange;
            if (length > 4)
                color = Color.Red;
            if (length > 5)
                color = Color.DarkRed;
            if (length > 6)
                color = Color.Purple;
            if(Runtime.Current.renderHost.info.frameCount % 16 == 0)
            if (j < lineCt)
                j++;
            else j = 0;
            for (int i = 0; i < lineCt; i++)
            {

                float x = j / lineCt;
                color = Pixel.Lerp(color, Color.Gray, x);
                // calculate start and end points of the line
                float startAngle = i * sliceAngle;
                Vector2 startPt = center + new Vector2(MathF.Cos(startAngle * CMath.PI / 180) * radius, (float)(Math.Sin(startAngle * CMath.PI / 180) * radius));
                Vector2 endPt = (startPt - center) * length + center;   
                ShapeDrawer.DrawLine(startPt, endPt, color);
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
