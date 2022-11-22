﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Windows.Controls.Image;
using System.Linq;
using Color = System.Drawing.Color;
using System.Windows.Controls;

namespace pixel_renderer
{
    /// <summary>
    /// Warning : This class consists of Unsafe code. Please ensure the project has 'Use Unsafe Code' enabled.
    /// </summary>
    public unsafe static class CBit
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr intPtr);

        [DllImport("PIXELRENDERER")]
        public unsafe static extern IntPtr GetHBITMAP(IntPtr intPtr, byte r, byte g, byte b);

        public unsafe static void BitmapToSource(Bitmap bmp, Image source)
        {
            var bitmapData = bmp.LockBits(
              new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
              System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            source.Source = BitmapSource.Create(
              bitmapData.Width, bitmapData.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
              bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            bmp.UnlockBits(bitmapData);
            // clean up unmanaged resource? i think?
            DeleteObject(bitmapData.Scan0);
        }
        [STAThread]
        public unsafe static void Draw(Stage stage, Bitmap bitmap)
        {
            Bitmap bmp = bitmap;

            Sprite sprite = new();

            IEnumerable<Sprite> sprites = from Node node in stage.Nodes
                                          where node.TryGetComponent<Sprite>(out sprite)
                                          select sprite;
       
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                  System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                  bmp.PixelFormat);

            // massively expensive, causes total hangups 
            var colors = new Color[Settings.ScreenWidth + Settings.ScreenHeight];

            // get sprite data in color array;
            foreach (Node node in stage.Nodes)
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        var offsetX = node.position.x + x;
                        var offsetY = node.position.y + y;

                        if (offsetX < 0) continue;
                        if (offsetY < 0) continue;

                        if (offsetX >= Settings.ScreenWidth) continue;
                        if (offsetY >= Settings.ScreenHeight) continue;

                        var color = sprite.colorData[x, y];

                        var pixelOffsetY = (int)offsetY;
                        var pixelOffsetX = (int)offsetX;

                        colors[pixelOffsetX + pixelOffsetY] = color;
                    }
                     // draw sprite data to bitmap unmanaged
                    byte[] row_ = new byte[bmd.Width * 4];
                    foreach (var pixel in colors)
                    {
                        for (var i = 0; i < bmp.Width; ++i)
                        {
                            var offset = i * 4;
                            row_[offset + 0] = pixel.B;
                            row_[offset + 1] = pixel.G;
                            row_[offset + 2] = pixel.R;
                            row_[offset + 3] = pixel.A;
                        }

                        for (var i = 0; i < bmd.Height; ++i)
                            Marshal.Copy(row_, 0, bmd.Scan0 + (i * bmd.Stride), row_.Length);
                    }

                    bmp.UnlockBits(bmd);
                    DeleteObject(bmd.Scan0);
        }
    }
}