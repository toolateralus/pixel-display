using Pixel;
using Pixel.Types.Components;
using System.Numerics;
using System.Windows.Input;
using Pixel.Types;
using static Pixel.Input;
using static Pixel.ShapeDrawer;
using static Pixel.Runtime;
using System;

namespace Pixel
{
    public class SineAnimationTests : Component
    {
        bool y_pressed = false;
        public override void Dispose()
        {
        }
        public override void Awake()
        {
            RegisterAction(this, OnKeyDown_Y, Key.Y);
            RegisterAction(this, OnKeyUp_Y, Key.Y, InputEventType.KeyUp);
        }
        public override void OnDestroy()
        {
            Log($"{node.Name} has been destroyed");
        }
        private void OnKeyUp_Y()
        {
            y_pressed = false;
        }
        private void OnKeyDown_Y()
        {
            if (y_pressed)
                return;
            y_pressed = true;
        }

        float radius = 2;
        float length = 0;

        private bool reversing;
        private bool animated = true;


        Color currentColor = Color.White;
        Vector2 endPt = default;
        public override void OnDrawShapes()
        {
            AnimateLength();
            RunAnimation();
        }
        private void RunAnimation()
        {
            int lineCt = (int)length.Squared() * 50;
            float sliceAngle = 360f / lineCt;

            for (int i = lineCt; i > 0; i--)
            {
                float startAngle = i * sliceAngle;
                float angle = startAngle * CMath.PI / 180;
                float cos = MathF.Cos(angle) * length * radius * 2;
                float sin = MathF.Sin(angle * length) * radius * 2;
                endPt = node.Position + new Vector2(cos, sin);

                var sine = MathF.Sin(angle);
                float depth = MathF.Abs(sine); // Calculate depth based on sine of angle
                if (sine < 0)
                    depth /= 3;

                currentColor = GetGradient(i, lineCt, alpha: 25);
                ShapeDrawer.DrawCircleFilled(endPt, 0.015f * depth, currentColor);
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

    }
}
