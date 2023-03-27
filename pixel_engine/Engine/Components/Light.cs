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
        private bool reversing;
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


            if (length > 7 && !reversing)
            {
                reversing = true;
            }

            if (reversing)
            {
                if (length <= 0)
                {
                    length = 0;
                    reversing = false; 
                }

                length -= 0.01f; 
            } else length += 0.01f;


            color = Pixel.Lerp(Color.Purple, Color.Red, length / 7);
           

            for (int i = 0; i < lineCt; i++)
            {
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
