﻿using System;
using System.Drawing;

namespace pixel_renderer
{
    internal class ImageScaling
    {
        internal static Bitmap Scale(Bitmap image, Vec2 desiredSize)
        {
            Bitmap destImage = new Bitmap(image.Width, image.Height);
            using (Graphics graphics = Graphics.FromImage(destImage))
                graphics.DrawImage(image, new Rectangle(0, 0, (int)desiredSize.x, (int)desiredSize.y), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            return destImage;
        }
    }
}