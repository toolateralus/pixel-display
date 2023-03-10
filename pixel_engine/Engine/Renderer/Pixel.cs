using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public struct Pixel
    {
        public byte a;
        public byte r;
        public byte g;
        public byte b;

        public readonly static Pixel White = new(255, 255, 255, 255);
        public readonly static Pixel Black = new(255, 0, 0, 0);
        public readonly static Pixel Green = new(255, 0, 255, 0);
        public readonly static Pixel Red = new(255, 255, 0, 0);
        public readonly static Pixel Blue = new(255, 0, 0, 255);

        public static Pixel operator *(Pixel a, Pixel b)
        {
            a.a *= b.a;
            a.r *= b.r;
            a.g *= b.g;
            a.b *= b.b;
            return a; 
        }
        public static Pixel operator *(Pixel A, float B)
        {
            A.a = (byte)(A.a * B);
            A.r = (byte)(A.r * B);
            A.g = (byte)(A.g * B);
            A.b = (byte)(A.b * B);
            return A;
        }
        public Pixel(byte a, byte r, byte g, byte b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }
        public static Pixel Lerp(Pixel A, Pixel B, float T)
        {
            T = Math.Max(0, Math.Min(1, T));
            byte r = (byte)Math.Round(A.r + (B.r - A.r) * T);
            byte g = (byte)Math.Round(A.g + (B.g - A.g) * T);
            byte b = (byte)Math.Round(A.b + (B.b - A.b) * T);
            byte a = (byte)Math.Round(A.a + (B.a - A.a) * T);
            return Pixel.FromArgb(a, r, g, b);
        }
        public static Pixel FromArgb(byte a, byte r, byte g, byte b)
        {
            return new(a, r, g, b);
        }
        public static implicit operator System.Drawing.Color(Pixel v) => Color.FromArgb(v.a, v.r, v.g, v.b );
        public static implicit operator Pixel(System.Drawing.Color v) => Pixel.FromArgb(v.A, v.R, v.G, v.B );

    }
}
