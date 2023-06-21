using Pixel.Types.Physics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;

namespace Pixel
{
    public static unsafe class CBit
    {
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr intPtr);

        public static unsafe void Render(Bitmap source, System.Windows.Controls.Image destination)
        {
            BitmapData bmd = source.LockBits(source.Rect(), ImageLockMode.ReadOnly, source.PixelFormat);
            destination.Source = BitmapSource.Create(
                bmd.Width, bmd.Height, 96, 96, source.PixelFormat.ToMediaFormat(), null,
                bmd.Scan0, bmd.Stride * bmd.Height, bmd.Stride);
            source.UnlockBits(bmd);
            DeleteObject(bmd.Scan0); //I don't actually know if this is neccesary, this might be costing some performance for no reason.;

        }
        public static byte[] ByteFromPixel(Color[,] colorArray)
        {
            var width = colorArray.GetLength(0);
            var height = colorArray.GetLength(1);
            byte[] pixelData = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = colorArray[x, y];
                    int index = (y * width + x) * 4;
                    pixelData[index] = color.a;
                    pixelData[index + 1] = color.r;
                    pixelData[index + 2] = color.g;
                    pixelData[index + 3] = color.b;
                }
            }
            return pixelData;
        }
        public static Color[,] PixelFromBitmap(Bitmap bmp, bool dispose = false)
        {
            if (bmp is null)
                throw new NullReferenceException("bitmap cannot be null.");

            lock (bmp)
            {
                int i = bmp.Width;
                int j = bmp.Height;

                if (bmp == null)
                    return new Color[0, 0];
                Color[,] colors = new Color[i, j];

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        colors[x, y] = bmp.GetPixel(x, y);
                    }
                }
                if (dispose)
                    bmp.Dispose();
                return colors;
            }
        }
        public static Bitmap SolidColorBitmap(Vector2 size, Color color)
        {
            int x = (int)size.X;
            int y = (int)size.Y;

            var bitmap = new Bitmap(x, y);

            for (int i = 0; i < x; i++)
                for (int j = 0; j < x; j++)
                    bitmap.SetPixel(i, j, color);
            return bitmap;
        }
        public static Color[,] SolidColorSquare(Vector2 size, Color color)
        {
            int sz_x = (int)Math.Floor(size.X);
            int sz_y = (int)Math.Floor(size.Y);

            var colorData = new Color[sz_x, sz_y];
            for (int x = 0; x < sz_x; x++)
                for (int y = 0; y < sz_y; y++)
                    colorData[x, y] = color;
            return colorData;
        }
        public static Color[,] SolidColorPolygon(Polygon polygon, Color color)
        {
            BoundingBox2D boundingRect = Polygon.GetBoundingBox(polygon.vertices);
            int width = (int)Math.Ceiling(boundingRect.Width);
            int height = (int)Math.Ceiling(boundingRect.Height);

            // Create the color data array
            var colorData = new Color[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the global position of the pixel
                    Vector2 globalPos = new Vector2(x + boundingRect.X, y + boundingRect.Y);

                    // Check if the pixel is inside the polygon
                    if (polygon.ContainsPoint(globalPos))
                    {
                        // Set the color for the pixel
                        colorData[x, y] = color;
                    }
                }
            }

            return colorData;
        }
        public static void RenderFromFrame(byte[] frame, int stride, Vector2 resolution, System.Windows.Controls.Image output)
        {
            // this (stride <= 0) is expected exactly once on startup, I don't know why.
            if (stride <= 0)
                return;
            output.Source = BitmapSource.Create((int)resolution.X, (int)resolution.Y, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null, frame, stride);
        }
        public static int GetStride(Vector2 resolution) => 4 * ((int)resolution.X * 24 + 31) / 32;

    }
}