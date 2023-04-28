using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Pixel
{
    public struct Color
    {
        /// <summary>
        /// shortcut to random pixel
        /// </summary>
        public static Color Random => JRandom.Pixel();

        /// <summary>
        /// it's not actually clear, just ARGB : 1,1,1,1
        /// </summary>
        public readonly static Color Clear = new(1, 1, 1, 1);


        public byte a;
        public byte r;
        public byte g;
        public byte b;

        public readonly static Color White = new(255, 255, 255, 255);
        public readonly static Color Black = new(255, 0, 0, 0);
        public readonly static Color Green = new(255, 0, 255, 0);
        public readonly static Color Red = new(255, 255, 0, 0);
        public readonly static Color Blue = new(255, 0, 0, 255);

        public static Color operator *(Color a, Color b)
        {
            a.a *= b.a;
            a.r *= b.r;
            a.g *= b.g;
            a.b *= b.b;
            return a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color operator *(Color A, float B)
        {
            A.a = (byte)(A.a * B);
            A.r = (byte)(A.r * B);
            A.g = (byte)(A.g * B);
            A.b = (byte)(A.b * B);
            return A;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color(byte a, byte r, byte g, byte b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Lerp(Color A, Color B, float T)
        {
            T = Math.Max(0, Math.Min(1, T));
            byte r = (byte)Math.Round(A.r + (B.r - A.r) * T);
            byte g = (byte)Math.Round(A.g + (B.g - A.g) * T);
            byte b = (byte)Math.Round(A.b + (B.b - A.b) * T);
            byte a = (byte)Math.Round(A.a + (B.a - A.a) * T);
            return Color.FromArgb(a, r, g, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(Color A, Color B, float T, out Color output)
        {
            output.r = (byte)(A.r + (B.r - A.r) * T);
            output.g = (byte)(A.g + (B.g - A.g) * T);
            output.b = (byte)(A.b + (B.b - A.b) * T);
            output.a = (byte)(A.a + (B.a - A.a) * T);
        }
        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new(a, r, g, b);
        }

        public void BlendTo(Color v, float weight)
        {
            byte r = (byte)((1 - weight) * this.r + weight * v.r);
            byte g = (byte)((1 - weight) * this.g + weight * v.g);
            byte b = (byte)((1 - weight) * this.b + weight * v.b);
            byte a = (byte)((1 - weight) * this.a + weight * v.a);
            
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;;
        }

        public static implicit operator System.Drawing.Color(Color v) => System.Drawing.Color.FromArgb(v.a, v.r, v.g, v.b);
        public static implicit operator Color(System.Drawing.Color v) => Color.FromArgb(v.A, v.R, v.G, v.B);

    }
}
