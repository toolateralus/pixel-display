using System;
using System.Drawing;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using pixel_renderer.Scripts;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(Metadata imgData, Metadata maskData, Color? color, string Name, string UUID) : base(Name, UUID)
        {
            this.imgData = imgData;
            this.maskData = maskData;
            this.color = color;
            this.Name = Name;
        }
        public Texture(Vec2Int scale, Metadata? imgData = null, Color? color = null)
        {
            this.scale = scale; 
            this.color = color;

            if (imgData is not null)
            {
                this.imgData = imgData;
                Image = new(imgData.fullPath);
            }
            else imgData = Player.test_image_data;

            if(color is not null)  Image = CBit.SolidColorBitmap(this.scale, (Color)color);
            
            
        }
         public Bitmap? Image { get; set; }

        [Field] public Bitmap? Mask;

        [JsonProperty] internal Metadata imgData;
        [JsonProperty] internal Metadata maskData;

        [Field] [JsonProperty] public Color? color;
        [Field] [JsonProperty] public Vec2Int scale = new(1, 1);
        
        public bool HasImage => Image != null;
        internal bool HasImageMetadata => imgData != null;

        public bool HasMask => Mask != null;
        internal bool HasMaskMetadata => imgData != null;

        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);
    }
}
