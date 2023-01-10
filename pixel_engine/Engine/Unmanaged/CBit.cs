using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
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
        public static unsafe void ReadonlyBitmapData(in Bitmap bmp, out BitmapData bmd, out int stride, out byte[] data)
        {
            Bitmap copy = bmp.Clone() as Bitmap;
            Rectangle rect = new(0, 0, copy.Width, copy.Height);
            bmd = copy.LockBits(new Rectangle(0, 0, copy.Width, copy.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            stride = bmd.Stride;
            data = new byte[stride * copy.Height];
            Marshal.Copy(bmd.Scan0, data, 0, data.Length);
            copy.UnlockBits(bmd);
            DeleteObject(bmd.Scan0);
        }
        /// <summary>
        /// a cheap way to draw a Bitmap image (in memory) to a Image control reference.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static unsafe void Render(ref Bitmap source, Image destination)
        {
            BitmapData bmd = GetWriteOnlyBitmapData(source);
            destination.Source = BitmapSource.Create(
                bmd.Width, bmd.Height, 96, 96, source.PixelFormat.ToMediaFormat(), null,
                bmd.Scan0, bmd.Stride * bmd.Height, bmd.Stride);
            source.UnlockBits(bmd);
            DeleteObject(bmd.Scan0);
        }
        /// <summary>
        ///  asseses each node in the stage and renders any neccesary data
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="bmp"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns> A modified input Bitmap that holds all the rendered data from the Stage</returns>
        internal static void Draw(IEnumerable<Sprite> sprites, Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArrayTypeMismatchException($"Bitmap.PixelFormat is {bmp.PixelFormat}, should be Format32bppArgb");
                return;
            }
            BitmapData bmd = GetWriteOnlyBitmapData(bmp);

            // draw sprite data to bitmap unmanaged

            foreach (Sprite sprite in sprites)
            {
                int posX = (int)sprite.parent.position.x;
                int posY = (int)sprite.parent.position.y;
                int width = sprite.colorData.GetLength(0);
                int height = sprite.colorData.GetLength(1);

                if (posX + width < 0 || posX >= bmd.Width) continue;
                if (posY + height < 0 || posY >= bmd.Height) continue;

                int leftMargin = Math.Max(0, -posX);
                int rightMargin = Math.Max(0, posX + width - bmd.Width);
                int rowSizeInBytes = (width - (leftMargin + rightMargin)) * 4;
                byte[] bytes = new byte[rowSizeInBytes];

                for (int localY = 0; localY < height; localY++)
                {
                    int y = localY + posY;
                    if (y < 0) continue;
                    if (y >= bmd.Height) break;
                    int localByteOffset = (localY * width + leftMargin) * 4;
                    Buffer.BlockCopy(sprite.colorData, localByteOffset, bytes, 0 , rowSizeInBytes);
                    IntPtr destination = bmd.Scan0 + (y * bmd.Stride) + (posX * 4);
                    Marshal.Copy(bytes, 0, destination, rowSizeInBytes);
                }
            }

            bmp.UnlockBits(bmd);
            DeleteObject(bmd.Scan0);
        }

        private static BitmapData GetWriteOnlyBitmapData(Bitmap bmp)
        {
            return bmp.LockBits(bmp.Rect(), ImageLockMode.WriteOnly, bmp.PixelFormat);
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

                        colors[(int)(offsetY * 255 + offsetX)] = sprite.colorData[x, y];
                    }
            return colors;
        }
        public static unsafe Color[,] ColorArrayFromBitmapData(BitmapData bmd, int stride, byte[] data)
        {
            int curRowOffs = 0;
            Color[,] _colors = new Color[bmd.Width, bmd.Height];

            for (int y = 0; y < bmd.Height; y++)
            {
                int byteOffset = curRowOffs;
                for (int x = 0; x < bmd.Width; x++)
                {
                    byte b = data[byteOffset];
                    byte g = data[byteOffset + 1];
                    byte r = data[byteOffset + 2];
                    byte a = data[byteOffset + 3];
                    byteOffset += 4;
                    _colors[x, y] = Color.FromArgb(a, r, g, b);
                }
                curRowOffs += stride;
            }

            return _colors;
        }
    }
}