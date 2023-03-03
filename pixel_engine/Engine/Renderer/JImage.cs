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
                if (size.X != width|| size.Y != height)
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
            byte[] byteData = CBit.ByteArrayFromColorArray(pixels);
            data = byteData;
        }
        public JImage(Bitmap bmpInput)
        {
            Pixel[,] pixels;
            lock (bmpInput)
                pixels = CBit.PixelArrayFromBitmap(bmpInput);

            width = pixels.GetLength(0);
            height = pixels.GetLength(1);

            byte[] byteData = CBit.ByteArrayFromColorArray(pixels);

            data = byteData;
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
        public  Pixel GetPixel(int x, int y)
        {

            int position = (y * width + x) * 4;
            var a = data[position + 0];
            var r = data[position + 1];
            var g = data[position + 2];
            var b = data[position + 3];
            Pixel col = new(a, r, g, b);
            return col;
        }

      
    }
}
