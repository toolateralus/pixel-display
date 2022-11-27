using System;
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
        internal static extern bool DeleteObject(IntPtr intPtr);

        [DllImport("PIXELRENDERER")]
        internal unsafe static extern IntPtr GetHBITMAP(IntPtr intPtr, byte r, byte g, byte b);

        public unsafe static void Render(ref Bitmap bmp, Image source)
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
        internal unsafe static void Draw(Stage stage, Bitmap bitmap)
        {
            Bitmap bmp = bitmap;

            Sprite sprite = new();

            IEnumerable<Sprite> sprites = from Node node in stage.Nodes
                                          where node.TryGetComponent<Sprite>(out sprite)
                                          select sprite;
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                  System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                  PixelFormat.Format32bppArgb);

            Color[] colors = ColorGraph( sprites);
            // draw sprite data to bitmap unmanaged
            byte[] colorBytes = new byte[bmd.Width * bmd.Height];

            for (var i = 0; i < bmd.Width * bmd.Height ; ++i)
            {
                var offset = i * 4;
                colorBytes[offset + 0] =255;
                colorBytes[offset + 1] =255;
                colorBytes[offset + 2] =255;
                colorBytes[offset + 3] = 255;
            }

            //for (var i = 0; i < bmd.Width; ++i)
            //{
            //    var offset = i * 4;
            //    colorBytes[offset + 0] = colors[offset].B;
            //    colorBytes[offset + 1] = colors[offset].G;
            //    colorBytes[offset + 2] = colors[offset].R;
            //    colorBytes[offset + 3] = colors[offset].A;
            //}

            int start = 0;
            int length = colorBytes.Length; 
            IntPtr destination = bmd.Scan0; 

            if (!length.Equals(Settings.ScreenW * Settings.ScreenH))
                throw new InvalidOperationException("Color array is not the appropriate size.");

            Marshal.Copy(colorBytes, start, destination, length);

          
            bmp.UnlockBits(bmd);
            DeleteObject(bmd.Scan0);
        }

        internal  static Color[] ColorGraph(IEnumerable<Sprite> sprites)
        {
            var colors = new Color[Settings.ScreenW *  Settings.ScreenH];
            foreach (Sprite sprite in sprites)
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        var offsetX = sprite.parentNode.position.x + x;
                        var offsetY = sprite.parentNode.position.y + y;

                        if (offsetX is < 0 or >= Settings.ScreenW
                            || offsetY is < 0 or >= Settings.ScreenH) continue;

                        colors[(int)(offsetY * 255  + offsetX)] =  sprite.colorData[x, y];
                    }
            return colors;
        }
    }
}