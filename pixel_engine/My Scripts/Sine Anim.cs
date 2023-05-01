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
    public class SineAnimator : Component
    {
        private float radius = 2;
        private float length = 0;
        private bool reversing;
        private Color currentColor = Color.White;
        private Vector2 endPt = default;
        public override void Dispose()
        {
        }
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
                float sin = MathF.Sin(angle * length * radius) * 2;
                endPt = node.Position + new Vector2(cos, sin);

                var sine = MathF.Sin(angle);
                float depth = MathF.Abs(sine); // Calculate depth based on sine of angle
                if (sine < 0)
                    depth /= 3;

                currentColor = Gradient.Sample(i, lineCt, alpha: 25);
                ShapeDrawer.DrawCircleFilled(endPt, 0.015f * depth, currentColor);
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
    }
}
