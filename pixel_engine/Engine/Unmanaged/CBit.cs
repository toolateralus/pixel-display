using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace pixel_renderer
{
    public static unsafe class CBit
    {
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr intPtr);
       
        /// <summary>
        /// a cheap way to draw a Bitmap image (in memory) to a Image control reference.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static unsafe void Render(Bitmap source, Image destination)
        {
            BitmapData bmd = source.LockBits(source.Rect(), ImageLockMode.ReadOnly, source.PixelFormat);
            destination.Source = BitmapSource.Create(
                bmd.Width, bmd.Height, 96, 96, source.PixelFormat.ToMediaFormat(), null,
                bmd.Scan0, bmd.Stride * bmd.Height, bmd.Stride);
            source.UnlockBits(bmd);
            DeleteObject(bmd.Scan0);

        }
       
        public static Color[,] ColorArrayFromBitmap(Bitmap bmp)
        {
            lock (bmp)
            {
                int i = bmp.Width;
                int j = bmp.Height;

                if (bmp == null)
                    return new Color[0, 0];
                Color[,] _colors = new Color[i,j];

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    { 
                        _colors[x, y] = bmp.GetPixel(x, y);
                    }
                }
                return _colors;
            }
        }
        public static Bitmap SolidColorBitmap(Vec2 size, Color color)
        {
            int x = (int)size.x;
            int y = (int)size.y;

            var bitmap = new Bitmap(x, y);

            for (int i = 0; i < x; i++)
                for (int j = 0; j < x; j++)
                    bitmap.SetPixel(i, j, color);
            return bitmap;
        }
        public static Color[,] SolidColorSquare(Vec2 size, Color color)
        {
            var colorData = new Color[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    colorData[x, y] = color;
            return colorData;
        }
        
        public static void RenderFromFrame(byte[] frame, int stride, Vec2 resolution, Image output)
        {
            if (stride <= 0)
            {
                Runtime.Log("Stride cannot be zero or less;");
                return; 
            }

            output.Source = BitmapSource.Create(
                (int)resolution.x, (int)resolution.y, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
                frame, stride);
        }
    }
}