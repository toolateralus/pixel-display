using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    internal class ISmage
    {

        public Metadata meta;

        public Vec2Int resolution => m_resolution;
        private readonly Vec2Int m_resolution;

        private int stride;
        private byte[] data;

        public ISmage(int width, int height, Bitmap input)
        {
            m_resolution = new(width, height);

            Bitmap scaledInput = new(input, width, height);
            Color[,] colors = CBit.ColorArrayFromBitmap(scaledInput);
            CBit.ByteArrayFromColorArray(colors, out data, out stride);
        }


    }
}
