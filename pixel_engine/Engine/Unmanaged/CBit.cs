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
        public static unsafe byte[] ReadonlyBitmapData(in Bitmap bmp, out BitmapData bmd)
        {
            bmd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] data = new byte[bmd.Stride * bmp.Height];
            Marshal.Copy(bmd.Scan0, data, 0, data.Length);
            bmp.UnlockBits(bmd);
            return data;
        }
        public static void RenderFromFrame(byte[] frame, int stride, Vec2 resolution, Image output)
        {
            output.Source = BitmapSource.Create(
                (int)resolution.x, (int)resolution.y, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
                frame, stride);
        }

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
        }
        public static void ByteArrayFromColorArray(Color[,] colors, out byte[] data, out int stride)
        {
            var w = colors.GetLength(0);
            var h = colors.GetLength(1);
            stride = 4 + (w * 24 + 31) / 32;
            data = new byte[stride * h];

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    data[y * stride + x * 3 + 0] = colors[x, y].B;
                    data[y * stride + x * 3 + 1] = colors[x, y].G;
                    data[y * stride + x * 3 + 2] = colors[x, y].R;
                }
        }

        /// <summary>
        ///  asseses each node in the stage and renders any neccesary data
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="bmp"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns> A modified input Bitmap that holds all the rendered data from the Stage</returns>
        internal static unsafe void Draw(IEnumerable<Sprite> sprites, Bitmap bmp)
        {
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                  System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                  System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // draw sprite data to bitmap unmanaged
            byte[] colorBytes = new byte[bmd.Width * bmd.Height];

            for (var i = 0; i < bmd.Width * bmd.Height; ++i)
            {
                if (i >= colorBytes.Length - 4) continue;
                colorBytes[i + 0] = 255;
                colorBytes[i + 1] = 15;
                colorBytes[i + 2] = 15;
                colorBytes[i + 3] = 15;
            }

            int start = 0;
            int length = colorBytes.Length;
            IntPtr destination = bmd.Scan0;

            if (!length.Equals(Constants.ScreenW * Constants.ScreenH))
                throw new InvalidOperationException("Color array is not the appropriate size.");

            //Marshal.Copy(colorBytes, start, destination, length);

            bmp.UnlockBits(bmd);
        }
        /// <summary>
        /// Takes a group of sprites and writes their individual color data arrays to a larger map as positions to prepare for collision and drawing.
        /// </summary>
        /// <param name="sprites"></param>
        /// <returns></returns>
        internal static Color[] ColorGraph(IEnumerable<Sprite> sprites)
        {
            var colors = new Color[Constants.ScreenW * Constants.ScreenH];
            foreach (Sprite sprite in sprites)
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        var offsetX = sprite.parent.position.x + x;
                        var offsetY = sprite.parent.position.y + y;

                        if (offsetX is < 0 or >= Constants.ScreenW
                            || offsetY is < 0 or >= Constants.ScreenH) continue;

                        colors[(int)(offsetY * 255 + offsetX)] = sprite.ColorData[x, y];
                    }
            return colors;
        }
        public static Color[,] ColorArrayFromBitmap(Bitmap bmp)
        {
            if (bmp == null)
                return new Color[0, 0];
            Color[,] _colors = new Color[bmp.Width, bmp.Height];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                { 
                    _colors[x, y] = bmp.GetPixel(x, y);
                }
            }
            return _colors;
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
    }
}