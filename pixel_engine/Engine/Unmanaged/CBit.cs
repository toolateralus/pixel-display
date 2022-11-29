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
        [DllImport("PIXELRENDERER")] internal unsafe static extern IntPtr GetHBITMAP(IntPtr intPtr, byte r, byte g, byte b);
        [DllImport("gdi32.dll")] internal static extern bool DeleteObject(IntPtr intPtr);
        public unsafe static void ReadonlyBitmapData( in Bitmap bmp ,out BitmapData bmd, out int stride, out byte[] data)
        {
            Bitmap copy = bmp.Clone() as Bitmap;
            Rectangle rect = new(0, 0, copy.Width, copy.Height);
            bmd = copy.LockBits(new Rectangle(0, 0, copy.Width, copy.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            stride = bmd.Stride;
            data = new byte[stride * copy.Height];
            Marshal.Copy(bmd.Scan0, data, 0, data.Length);
            copy.UnlockBits(bmd);
        }
        /// <summary>
        /// a cheap way to draw a Bitmap image (in memory) to a Image control reference.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="source"></param>
         public unsafe static void Render(ref Bitmap bmp, Image source)
        {
            var bmd = bmp.LockBits(
              new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
              System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            source.Source = BitmapSource.Create(
              bmd.Width, bmd.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
              bmd.Scan0, bmd.Stride * bmd.Height, bmd.Stride);
            bmp.UnlockBits(bmd);
            DeleteObject(bmd.Scan0);
        }
        /// <summary>
        ///  asseses each node in the stage and renders any neccesary data
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="bmp"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns> A modified input Bitmap that holds all the rendered data from the Stage</returns>
        internal unsafe static void Draw(Stage stage, Bitmap bmp)
        {
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

            // test loop to set all bytes to White; 
            //for (var i = 0; i < bmd.Width * bmd.Height ; ++i)
            //{
            //    var offset = i * 4;
            //    colorBytes[offset + 0] = 255;
            //    colorBytes[offset + 1] = 255;
            //    colorBytes[offset + 2] = 255;
            //    colorBytes[offset + 3] = 255;
            //}
            for (var i = 0; i < bmd.Width * bmd.Height; ++i)
            {
                var offset = i * 4;
                colorBytes[offset + 0] = colors[offset].B;
                colorBytes[offset + 1] = colors[offset].G;
                colorBytes[offset + 2] = colors[offset].R;
                colorBytes[offset + 3] = colors[offset].A;
            }

            int start = 0;
            int length = colorBytes.Length; 
            IntPtr destination = bmd.Scan0; 

            if (!length.Equals(Settings.ScreenW * Settings.ScreenH))
                throw new InvalidOperationException("Color array is not the appropriate size.");

            Marshal.Copy(colorBytes, start, destination, length);
            
            bmp.UnlockBits(bmd);
            DeleteObject(bmd.Scan0);
        }
        /// <summary>
        /// Takes a group of sprites and writes their individual color data arrays to a larger map to prepare for collision and drawing.
        /// </summary>
        /// <param name="sprites"></param>
        /// <returns></returns>
        internal static Color[] ColorGraph(IEnumerable<Sprite> sprites)
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
        public static unsafe Color[,] ColorArrayFromBitmapData(BitmapData bmd, int stride, byte[] data)
        {
            int curRowOffs = 0;
            Color[,] _colors = new Color[bmd.Width, bmd.Height];

            for (int y = 0; y < bmd.Height; y++)
            {
                int curOffs = curRowOffs;
                for (int x = 0; x < bmd.Width; x++)
                {
                    byte b = data[curOffs];
                    byte g = data[curOffs + 1];
                    byte r = data[curOffs + 2];
                    byte a = data[curOffs + 3];
                    curOffs += 4;
                    _colors[x, y] = Color.FromArgb(a, r, g, b);
                }
                curRowOffs += stride;
            }

            return _colors;
        }
    }
}