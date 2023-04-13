using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
    public class Light : Component
    {
        [Field] public float brightness = 25f;
        [Field] public Vector2 startPos = Vector2.One;
        [Field] public float radius = 55;
        [Field] public Pixel color = ExtensionMethods.Lerp(Color.White, Color.Yellow, 0.7f);
        
        private bool reversing;
        float length = 0;
        Color[] gradientColors = { Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Magenta };
        private bool animated = true;

        public override void OnDrawShapes()
        {
            const int lineCt = 120;
            const float sliceAngle = 360f / lineCt; // divide circle into 6 equal slices
            const float radius = 5f; // radius of the circle
            var center = node.Position;
            
            if(animated)
                AnimateLength();

            for (int i = 0; i < lineCt; i++)
            {
                float startAngle = i * sliceAngle;
                Vector2 startPt = center + new Vector2(MathF.Cos(startAngle * CMath.PI / 180) * radius, (float)(Math.Sin(startAngle * CMath.PI / 180) * radius));
                Vector2 endPt = center + new Vector2(MathF.Cos(startAngle * CMath.PI / 180) * radius * length, (float)(Math.Sin(startAngle * CMath.PI / 180) * radius * length));

                float gradientPos = (float)i / lineCt;
                int colorSegment = (int)(gradientPos * (gradientColors.Length - 1));
                float segmentPos = (gradientPos * (gradientColors.Length - 1)) - colorSegment;
                Pixel currentColor = Pixel.Lerp(gradientColors[colorSegment], gradientColors[colorSegment + 1], segmentPos);

                currentColor.a = 80;

                ShapeDrawer.DrawLine(endPt, startPt /2 , currentColor);
            }
        }

        private void AnimateLength()
        {
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
            }
            else length += 0.01f;
        }

        public static Node Standard()
        {
            Node x = new("Light");
            x.AddComponent<Light>();
            return x; 
        }
    }
}
