using System.Drawing;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(Metadata imgData, Vec2Int scale, string Name = "Texture Asset") : base(Name, true)
        {
            this.imgData = imgData;
            this.Name = Name;
            this.scale = scale; 
            SetImage(imgData, scale);
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

        public Texture(Vec2Int scale, Metadata imgData)
        {
            SetImage(imgData, scale);
        }

        [Field] [JsonProperty] public Vec2Int scale = new(1, 1);
        [JsonProperty] internal Metadata imgData;
        public Bitmap? Image { get; set; }
        public bool HasImage => Image != null;
        internal bool HasImageMetadata => imgData != null;
        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);
    }
}
