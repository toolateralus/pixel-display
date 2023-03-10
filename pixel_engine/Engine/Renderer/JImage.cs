using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JImage
    {
        [JsonProperty]
        public readonly int width;
        [JsonProperty]
        public readonly int height;
        [JsonProperty]
        internal byte[] data;
        Vector2 size = new();
        public Vector2 Size
        {
            get
            {
                if (size.X != width || size.Y != height)
                    size = new(width, height);
                return size;
            }
        }

        public JImage()
        {
            width = 0;
            height = 0;
            data = Array.Empty<byte>();
        }
        public JImage(Vector2 size, byte[] data)
        {
            width = (int)size.X;
            height = (int)size.Y;
            this.data = data;
        }
        public JImage(Pixel[,] pixels)
        {
            width = pixels.GetLength(0);
            height = pixels.GetLength(1);
            byte[] byteData = CBit.ByteFromPixel(pixels);
            data = byteData;
        }
        public JImage(Bitmap bmpInput)
        {
            if (bmpInput is null)
                return; 

            Pixel[,] pixels;
            lock (bmpInput)
                pixels = CBit.PixelFromBitmap(bmpInput);

            width = pixels.GetLength(0);
            height = pixels.GetLength(1);

            byte[] byteData = CBit.ByteFromPixel(pixels);

            data = byteData;
            //bmpInput.Dispose();
        }
        public JImage(BoundingBox2D bounds, byte[] bytes)
        {
            height = (int)bounds.Height;
            width = (int)bounds.Width;
            data = bytes; 
        }

        public void SetPixel(int x, int y, Pixel color)
        {
            int position = (y * width + x) * 4;
            if (data.Length < position + 4)
                throw new InvalidOperationException("image did not contain requested pixel");

            data[position + 0] = color.a;
            data[position + 1] = color.r;
            data[position + 2] = color.g;
            data[position + 3] = color.b;
        }
        public Pixel GetPixel(int x, int y)
        {
            if (data.Length == 0)
                return Pixel.Black;

            int position = (y * width + x) * 4;
            var a = data[position + 0];
            var r = data[position + 1];
            var g = data[position + 2];
            var b = data[position + 3];
            Pixel col = new(a, r, g, b);
            return col;
        }
        internal static JImage Concat(IReadOnlyCollection<JImage> images, Curve posCurve)
        {
            byte[] drawSurface = GetDrawSurface(images, posCurve, out var bounds);
            foreach (var image in images)
            {
                var position = posCurve.Next();

                var startX = Math.Max(0, (int)Math.Floor(position.X));
                var startY = Math.Max(0, (int)Math.Floor(position.Y));
                var endX = Math.Min((int)bounds.Width, startX + image.width);
                var endY = Math.Min((int)bounds.Height, startY + image.height);

                for (int x = startX; x < endX -1 ; x++)
                {
                    for (int y = startY; y < endY -1; y++)
                    {
                        var pxPos = new Vector2(x, y);

                        int mapPos = ((int)pxPos.Y * (int)bounds.Width + (int)pxPos.X) * 4;

                        if (3 + mapPos > drawSurface.Length) 
                            continue;

                        int imgPos = ((y - startY) * image.width + (x - startX)) * 4;
                        
                        if (3 + imgPos > image.data.Length)
                            continue;

                        drawSurface[mapPos + 0] = image.data[imgPos + 0];
                        drawSurface[mapPos + 1] = image.data[imgPos + 1];
                        drawSurface[mapPos + 2] = image.data[imgPos + 2];
                        drawSurface[mapPos + 3] = image.data[imgPos + 3];
                    }
                }
            }

            return new JImage(new Vector2(bounds.Width, bounds.Height), drawSurface);
        }
        private static byte[] GetDrawSurface(IReadOnlyCollection<JImage> images, Curve posCurve, out BoundingBox2D bounds)
        {
            bounds = new BoundingBox2D(0, 0, 1, 1);
            var pos = Vector2.Zero;
            foreach (var img in images)
            {
                pos = posCurve.Next();
                bounds.ExpandTo(pos);
                bounds.ExpandTo(pos + img.Size);
            }
            posCurve.Reset();
            byte[] drawSurface = new byte[(int)bounds.Width * (int)bounds.Height * 4];
            return drawSurface;
        }
    }
}
