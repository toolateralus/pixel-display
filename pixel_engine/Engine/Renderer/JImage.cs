using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer.Engine.Renderer
{
    public class JImage
    {
        public int width;
        public int height;
        public byte[] data;
        public JImage(Pixel[,] data)
        {

        }

        public void SetPixel(int x, int y, Pixel color)
        {
            int position = (y * width * 4) + (x * 4);
            if (data.Length < position + 4)
                throw new InvalidOperationException("image did not contain requested pixel");

            data[position + 0] = color.a;
            data[position + 1] = color.r;
            data[position + 2] = color.g;
            data[position + 3] = color.b;
        }
        public Pixel GetPixel(int x, int y)
        {
            int position = (y * width * 4) + (x * 4);
            var a = data[position + 0];
            var r = data[position + 1];
            var g = data[position + 2];
            var b = data[position + 3];
            Pixel col = new(a, r, g, b);
            return col;
        }
        public JImage(int width, int height, byte[] data)
        {
            this.width = width;
            this.height = height;
            this.data = data;
        }
    }
}
