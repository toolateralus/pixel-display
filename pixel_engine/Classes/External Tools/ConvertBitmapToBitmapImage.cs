using System;
using System.IO;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using System.Drawing.Imaging;
using Image = System.Windows.Controls.Image; 
public static class CBitmap
{
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr intPtr);

    public unsafe static void Convert(Bitmap bmp, Image source)
    {
        var bitmapData = bmp.LockBits(
          new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
          System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
         source.Source = BitmapSource.Create(
           bitmapData.Width, bitmapData.Height, 2, 2, System.Windows.Media.PixelFormats.Bgr24, null,
           bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            bmp.UnlockBits(bitmapData);
            // clean up unmanaged resource? i think?
             DeleteObject(bitmapData.Scan0);
    }

}
