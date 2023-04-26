using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace pixel_core
{
    [JsonObject(MemberSerialization.OptIn)]
    public unsafe class JImage
    {
        [JsonProperty]
        public readonly int width;
        [JsonProperty]
        public readonly int height;
        [JsonProperty]
        internal byte[] data;
        [JsonProperty]
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

        [JsonProperty]
        public Pixel color = Pixel.Clear; 

        public JImage()
        {
            width = 0;
            height = 0;
            data = Array.Empty<byte>();
        }
        public JImage(Vector2 size, byte[] colorData)
        {
            width = (int)size.X;
            height = (int)size.Y;
            ApplyColor(colorData);
            this.data = colorData;
        }
        public JImage(Pixel[,] pixels)
        {
            width = pixels.GetLength(0);
            height = pixels.GetLength(1);
            byte[] byteData = CBit.ByteFromPixel(pixels);
            ApplyColor(byteData);
            data = byteData;
        }

        private void ApplyColor(byte[] colorData)
        {
            for (int i = 0; i < colorData.Length; i += 4)
            {
                colorData[i] *= color.a; 
                colorData[i + 1] *= color.r;
                colorData[i + 2] *= color.g;
                colorData[i + 3] *= color.b;
            }
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
            byte[] colorData = CBit.ByteFromPixel(pixels);
            ApplyColor(colorData);
            data = colorData;
        }
        public JImage(BoundingBox2D bounds, byte[] colorData)
        {
            height = (int)bounds.Height;
            width = (int)bounds.Width;
            ApplyColor(colorData);
            data = colorData; 
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
            int position = (y * width + x) * 4;
            if (data is null || data.Length == 0 || data.Length < position + 4)
                return Pixel.Black;

            var a = data[position + 0];
            var r = data[position + 1];
            var g = data[position + 2];
            var b = data[position + 3];
            Pixel col = new(a, r, g, b);


            return col;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetPixel(int x, int y, out Pixel output)
        {
            int position = (y * width + x) * 4;
            output.a = data[position + 0];
            output.r = data[position + 1];
            output.g = data[position + 2];
            output.b = data[position + 3];
        }
        internal static JImage Concat(IReadOnlyCollection<JImage> images, Curve posCurve)
        {
            byte[] drawSurface = GetDrawSurface(images, posCurve, out var bounds);
            foreach (var image in images)
            {
                var position = posCurve.GetValue(true);

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
                pos = posCurve.GetValue(true);
                bounds.ExpandTo(pos);
                bounds.ExpandTo(pos + img.Size);
            }
            posCurve.Reset(); 
            byte[] drawSurface = new byte[(int)bounds.Width * (int)bounds.Height * 4];
            return drawSurface;
        }
        internal JImage Clone()
        {
            return (JImage)MemberwiseClone();
        }
        internal void NormalizeAlpha(int value)
        {
            for (int i = 0; i < data.Length; i += 4)
                data[i] = (byte)value;
        }
    }
}
