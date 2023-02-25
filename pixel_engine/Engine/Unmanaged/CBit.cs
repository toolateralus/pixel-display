using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Bitmap = System.Drawing.Bitmap;
using Pixel = System.Drawing.Color;
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
        public static byte[] ByteArrayFromColorArray(Pixel[,] colorArray)
        {
            var width = colorArray.GetLength(0);
            var height = colorArray.GetLength(1);
            byte[] pixelData = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Pixel color = colorArray[x, y];
                    int index = (y * width + x) * 4;
                    pixelData[index] =     color.r;
                    pixelData[index + 1] = color.g;
                    pixelData[index + 2] = color.b;
                    pixelData[index + 3] = color.a;
                }
            }
            return pixelData; 
        }

        public static Pixel[,] PixelArrayFromBitmap(Bitmap bmp)
        {
            lock (bmp)
            {
                int i = bmp.Width;
                int j = bmp.Height;

                if (bmp == null)
                    return new Pixel[0, 0];
                Pixel[,] colors = new Pixel[i,j];

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        colors[x, y] = bmp.GetPixel(x, y);
                    }
                }
                return colors;
            }
        }
        public static Bitmap SolidColorBitmap(Vec2 size, Pixel color)
        {
            int x = (int)size.x;
            int y = (int)size.y;

            var bitmap = new Bitmap(x, y);

            for (int i = 0; i < x; i++)
                for (int j = 0; j < x; j++)
                    bitmap.SetPixel(i, j, color);
            return bitmap;
        }
        public static Pixel[,] SolidColorSquare(Vec2 size, Pixel color)
        {
            var colorData = new Pixel[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    colorData[x, y] = color;
            return colorData;
        }
        
        public static void RenderFromFrame(byte[] frame, int stride, Vec2 resolution, Image output)
        {

            if (GetStride(resolution) != stride)
            {

            }
            if (stride <= 0)
            {
                Runtime.Log("Stride cannot be zero or less;");
                return;
            }

            try
            {
                output.Source = BitmapSource.Create(
                (int)resolution.x, (int)resolution.y, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
                frame, stride);
            }
            catch
            {

            }
            
        }

        public static int GetStride(Vec2 resolution)
        {
            return 4 * ((int)resolution.x * 24 + 31) / 32;
        }
    }
}