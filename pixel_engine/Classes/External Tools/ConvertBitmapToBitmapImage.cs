using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Windows.Controls.Image; 
public unsafe static class CBit
{
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr intPtr);
    
    [DllImport("PIXELRENDERER")]
    public unsafe static extern IntPtr GetHBITMAP(IntPtr intPtr, byte r, byte g, byte b);
    
    public unsafe static void Convert(Bitmap bmp, Image source)
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

}
