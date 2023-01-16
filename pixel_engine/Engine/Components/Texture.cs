using System;
using System.Drawing;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(Metadata imgData, Metadata maskData, Color? color, string Name, Type fileType, string? UUID = null) : base(Name, UUID)
        {

        }
        public Texture(Vec2 scale, Metadata? imgData = null, Color? color = null)
        {
            this.scale = scale; 
            this.color = color;
            if (imgData is not null)
            {
                this.imgData = imgData;
                Image = new(imgData.fullPath);
                runtime_img = GetScaledBitmap();
            }
            if(color is not null)
                Image = Sprite.SolidColorBitmap(this.scale, (Color)color);
            
        }
        [Field] public Bitmap? Image;
        [Field] public Bitmap? runtime_img; 

        [Field] public Bitmap? Mask;
        [Field] public Bitmap? runtime_mask; 

        [JsonProperty] internal Metadata imgData;
        [JsonProperty] internal Metadata maskData;

        [Field] [JsonProperty] public Color? color;
        [Field] [JsonProperty] public Vec2 scale = Vec2.one;
        
        public bool HasImage => Image != null;
        internal bool HasImageMetadata => imgData != null;

        public bool HasMask => Mask != null;
        internal bool HasMaskMetadata => imgData != null;

        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);
        public Color[,] GetColorArray()
        {
            if (Image is null)
                throw new Exception();

            Bitmap? copy = null;
            // clone the bitmap to prevent usage violations
            lock (Image)
                copy = (Bitmap)Image.Clone();

            Color[,] output = new Color[copy.Width, copy.Height];
            for (int i = 0; i < copy.Width; ++i)
                for (int j = 0; j < copy.Height; ++j)
                    output[i,j] = copy.GetPixel(i, j);
            return output; 
        }
    }
}
