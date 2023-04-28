using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Drawing;
using System.Numerics;

namespace Pixel
{
    public class Light : Component
    {
        [Field] public float heightModifier = 2.5f;
        [Field] public float brightness = 25f;
        [Field] public Vector2 startPos = Vector2.One;
        [Field] public float radius = 55;
        [Field] public Color color = ExtensionMethods.Lerp(System.Drawing.Color.White, System.Drawing.Color.Yellow, 0.7f);
        [Field] public bool showDebug = true;
        float length = 0;
        private bool reversing;
        private bool animated = true;
        Curve curve = Curve.Linear(-Vector2.One, Vector2.One, 1, 250);
        public override void Dispose()
        {

        }
        public override void OnDrawShapes()
        {
            if (!showDebug) return;
            var center = node.Position;

            if (animated)
                AnimateLength();

            int lineCt = (int)(52 * (length * length));
            float sliceAngle = 360f / lineCt; // divide circle into 6 equal slices

            for (int i = 0; i < lineCt; i++)
            {
                float startAngle = i * sliceAngle;
                float angle = startAngle * CMath.PI / 180;

                float cos = MathF.Cos(angle) * length * radius;
                double sin = Math.Sin(angle * length) * radius;

                Vector2 A = (new Vector2(cos, (float)sin));
                Vector2 startPt = center;
                Vector2 endPt = startPt + A * 2;

                Color currentColor = GetGradient(i, lineCt, alpha: 25);

                Interop.DrawCircleFilled(endPt, 0.015f, currentColor);
            }
        }

        /// <summary>
        /// <code>
        ///     Position = the index of the sample to return (between 0 and subdivisions - 1)
        ///     Subdivisions = the amount of positions possible to sample on the gradient
        ///     Alpha = the transparency of the output colors (0 - 255)
        ///     GradientColors = the array to sample
        /// </code>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="subdivisions"></param>
        /// <param name="alpha"></param>
        /// <param name="gradientColors"></param>
        /// <returns></returns>
        public static Color GetGradient(int position, int subdivisions = 360, byte alpha = 255, Color[]? gradientColors = null)
        {
            if (position >= subdivisions)
                position = subdivisions - 1;

            gradientColors ??= new Color[] { System.Drawing.Color.Red, System.Drawing.Color.Yellow, System.Drawing.Color.Green, System.Drawing.Color.Cyan, System.Drawing.Color.Blue, System.Drawing.Color.Magenta };
            float gradientPos = (float)position / subdivisions;
            int colorSegment = (int)(gradientPos * (gradientColors.Length - 1));
            float segmentPos = (gradientPos * (gradientColors.Length - 1)) - colorSegment;
            Color currentColor = Color.Blend(gradientColors[colorSegment], gradientColors[colorSegment + 1], segmentPos);
            currentColor.a = alpha;
            return currentColor;
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
