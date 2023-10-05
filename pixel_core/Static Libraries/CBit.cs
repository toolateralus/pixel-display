using Pixel.Types.Physics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Pixel
{
    
    // TODO: reimplment the image stuff here, although we shouldn't have to do any of that silly color conversion with the plans ahead
    public static unsafe class CBit
    {
        [DllImport("gdi32.dll")] // is this windows only?
        internal static extern bool DeleteObject(IntPtr intPtr);
        public static Color[,] SolidColorSquare(Vector2 size, Color color)
        {
            int sz_x = (int)Math.Floor(size.X);
            int sz_y = (int)Math.Floor(size.Y);

            var colorData = new Color[sz_x, sz_y];
            for (int x = 0; x < sz_x; x++)
                for (int y = 0; y < sz_y; y++)
                    colorData[x, y] = color;
            return colorData;
        }
        public static Color[,] SolidColorPolygon(Polygon polygon, Color color)
        {
            BoundingBox2D boundingRect = Polygon.GetBoundingBox(polygon.vertices);
            int width = (int)Math.Ceiling(boundingRect.Width);
            int height = (int)Math.Ceiling(boundingRect.Height);

            // Create the color data array
            var colorData = new Color[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the global position of the pixel
                    Vector2 globalPos = new Vector2(x + boundingRect.X, y + boundingRect.Y);

                    // Check if the pixel is inside the polygon
                    if (polygon.ContainsPoint(globalPos))
                    {
                        // Set the color for the pixel
                        colorData[x, y] = color;
                    }
                }
            }

            return colorData;
        }
        public static void RenderFromFrame(byte[] frame, int stride, Vector2 resolution)
        {
            // this (stride <= 0) is expected exactly once on startup, I don't know why.
            if (stride <= 0)
                return;
                
            // todo: implement rendering method here too.
        }
        public static int GetStride(Vector2 resolution) => 4 * ((int)resolution.X * 24 + 31) / 32;

    }
}