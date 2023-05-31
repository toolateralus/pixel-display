using Pixel.Types.Components;
using System.Numerics;
using System.Windows.Input;
using Pixel.Types;
using static Pixel.Input;
using static Pixel.ShapeDrawer;
using static Pixel.Runtime;
using System;
using System.Runtime.CompilerServices;
using System.Drawing.Printing;
using System.CodeDom;
using System.Drawing.Drawing2D;
using System.Linq;
using Newtonsoft.Json;

namespace Pixel
{
    public static class Sine
    {
        private const int count = 360;

        public static readonly double[] sin = new double[count];
        public static readonly double[] cos = new double[count];
        static Sine()
        {
            for (int i = 0; i < count; ++i)
            {
                cos[i] = Math.Cos(i);
                sin[i] = Math.Sin(i);
            }

        }
        public static float Sin(float angle)
        {
            angle = ((angle % 360) + 360) % 360;
            int i = (int)angle;
            int j = (i + 1) % count;
            float weight = angle - i;
            return (float)((1 - weight) * sin[i] + weight * sin[j]);
        }
        public static float Cos(float angle)
        {
            angle = ((angle % 360) + 360) % 360;
            int i = (int)angle;
            int j = (i + 1) % count;
            float weight = angle - i;
            return (float)((1 - weight) * cos[i] + weight * cos[j]);
        }
        public static (Animation<Vector3> pos, Animation<Color> col) GetColorfulSineWaveAnim(int length, float radius, byte alpha = 255, int frameTime = 24)
        {
            float sliceAngle = 360f / length;
            
            Vector3[] positions = new Vector3[length + 1];
            Color[] col = new Color[length + 1];
            
            Animation<Vector3> pos;
            Animation<Color> colors;
            
            col[0] = Color.White;
            positions[0] = new(radius, 0, 0);
            
            Vector2 position = default;
            Color color = default;

            for (int i = length; i > 0; i--)
            {
                float startAngle = i * sliceAngle;
                float angle = startAngle * CMath.PI / 180;
                float cos_val = Cos(angle) * radius;
                float sin_val = Sin(angle) * radius;

                position.X = cos_val;
                position.Y = sin_val;

                var sine = Sin(angle);

                float depth = MathF.Abs(sine);

                if (sine < 0)
                    depth /= 3;

                color = Gradient.Sample(i, length, alpha: alpha, GridNodeGenerator.Pallette);

                col[i] = color;

                positions[i] = new(position, depth);
            }

            colors = new(col, frameTime);

            pos = new(positions, frameTime);

            return (pos, colors);
        }
    }

        

}
