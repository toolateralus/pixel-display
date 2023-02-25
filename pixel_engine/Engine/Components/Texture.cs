using System;
using System.Drawing;
using System.Windows.Media;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(JImage image, Metadata imgData, Vec2Int scale, string Name = "Texture Asset") : base(Name, true)
        {
            this.imgData = imgData;
            this.Name = Name;
            this.scale = scale;
            this.jImage = image; 
        }
        public void SetImage(Metadata imgData, Vec2Int scale)
        {
            this.scale = scale;

            if (imgData is not null)
            {
                this.imgData = imgData;
                Image = new(imgData.Path);
            }
            else
            {
                this.imgData = Player.PlayerSprite;
                Image = new(imgData.Path);
            }
          
        }
        public void SetImage(Pixel[,] colors)
        {
            jImage = new(colors);
        }
        public void SetImage(Pixel color)
        {
            jImage = new(CBit.SolidColorSquare(scale, color));
        }
        public Texture(Vec2Int scale, Metadata imgData)
        {
            SetImage(imgData, scale);
        }
        public Texture(Vec2Int size, Pixel color)
        {
            scale = size;
            SetImage(color);
        }
       
        [Field] 
        [JsonProperty] 
        public Vec2Int scale = new(1, 1);
        
        [JsonProperty] 
        internal Metadata imgData;
        
        [JsonProperty]
        public JImage jImage = new();
        
        Bitmap image;
      
        public Bitmap? Image {
            get 
            {
                if (!HasImage && HasImageMetadata)
                    image = new(imgData.Path);
                return image;
            }
            set => image = value;
        }
        public bool HasImage => image != null;
        internal bool HasImageMetadata => imgData != null;
        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);
    }
}
