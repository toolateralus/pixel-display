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
using System.Numerics;

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
        public static byte[] ByteFromPixel(Pixel[,] colorArray)
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
                    pixelData[index] =     color.a;
                    pixelData[index + 1] = color.r;
                    pixelData[index + 2] = color.g;
                    pixelData[index + 3] = color.b;
                }
            }
            return pixelData; 
        }

        public static Pixel[,] PixelFromBitmap(Bitmap bmp, bool dispose = false)
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
                if(dispose)
                    bmp.Dispose();
                return colors;
            }
        }
        public static Bitmap SolidColorBitmap(Vector2 size, Pixel color)
        {
            int x = (int)size.X;
            int y = (int)size.Y;

            var bitmap = new Bitmap(x, y);

            for (int i = 0; i < x; i++)
                for (int j = 0; j < x; j++)
                    bitmap.SetPixel(i, j, color);
            return bitmap;
        }
        public static Pixel[,] SolidColorSquare(Vector2 size, Pixel color)
        {
            var colorData = new Pixel[(int)size.X, (int)size.Y];
            for (int x = 0; x < size.X; x++)
                for (int y = 0; y < size.Y; y++)
                    colorData[x, y] = color;
            return colorData;
        }
        
        public static void RenderFromFrame(byte[] frame, int stride, Vector2 resolution, Image output)
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
                (int)resolution.X, (int)resolution.Y, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
                frame, stride);
            }
            catch
            {

            }
            
        }

        public static int GetStride(Vector2 resolution)
        {
            return 4 * ((int)resolution.X * 24 + 31) / 32;
        }
    }
}