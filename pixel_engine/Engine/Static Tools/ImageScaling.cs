using System;
using System.Drawing;

namespace pixel_renderer
{
    internal class ImageScaling
    {
        internal static Bitmap Scale(Bitmap image, Vec2Int desiredSize)
        {
            Bitmap destImage = new Bitmap(desiredSize.x, desiredSize.y);
            using (Graphics graphics = Graphics.FromImage(image))
                graphics.DrawImage(image, new Rectangle(0, 0, (int)desiredSize.x, (int)desiredSize.y), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            return destImage;
        }
    }
}