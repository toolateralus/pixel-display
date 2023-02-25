using System.Drawing;
using Newtonsoft.Json;
using pixel_renderer.Engine.Renderer;
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

            jImage = new(CBit.PixelArrayFromBitmap(Image));
        }

        public Texture(Vec2Int scale, Metadata imgData)
        {
            SetImage(imgData, scale);
        }
        public Texture(Vec2Int scale, Pixel? color = null, Pixel[,] colors = null)
        {
            if (color != null)
            {
                jImage = new(scale.x, scale.y, CBit.ByteArrayFromColorArray(CBit.SolidColorSquare(scale, (Pixel)color)));
            }
            else if (colors != null)
                jImage = new(scale.x, scale.y, CBit.ByteArrayFromColorArray(colors));
         
        }

        [Field] [JsonProperty] public Vec2Int scale = new(1, 1);
        [JsonProperty] internal Metadata imgData;
        [JsonProperty] public JImage jImage;
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
