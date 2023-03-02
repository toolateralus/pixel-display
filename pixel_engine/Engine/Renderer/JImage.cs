﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public Vec2Int Size => new(width, height);

        public JImage()
        {
            width = 0;
            height = 0;
            data = Array.Empty<byte>();
        }
        public JImage(Vec2Int size, byte[] data)
        {
            width = size.x;
            height = size.y;
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
            if (width + height == 0)
                return Pixel.Black; 

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
