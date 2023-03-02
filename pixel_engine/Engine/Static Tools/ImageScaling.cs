using System;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
    internal class ImageScaling
    {
        internal static Bitmap Scale(Bitmap image, Vector2 desiredSize)
        {
            Bitmap destImage = new Bitmap((int)desiredSize.X ,(int)desiredSize.Y);
            using (Graphics graphics = Graphics.FromImage(image))
                graphics.DrawImage(image, new Rectangle(0, 0, (int)desiredSize.X ,(int)desiredSize.Y), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            return destImage;
        }
    }
}