using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Media;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(JImage image, Metadata imgData, Vector2 scale, string Name = "Texture Asset") : base(Name, true)
        {
            this.imgData = imgData;
            this.Name = Name;
            this.scale = scale;
            this.jImage = image; 
        }
        public Texture(Vector2 scale, Metadata imgData)
        {
            SetImage(imgData, scale);
        }
        public Texture(Vector2 size, Pixel color)
        {
            scale = size;
            SetImage(color);
        }
        [Field] 
        [JsonProperty] 
        public Vector2 scale = new(1, 1);
        [JsonProperty] 
        internal Metadata imgData;
        [JsonProperty]
        private JImage jImage = new();
        Bitmap initializedBitmap;
        public Bitmap? Image {
            get 
            {
                if (!HasImage && HasImageMetadata)
                    initializedBitmap = new(imgData.Path);
                return initializedBitmap;
            }
            set => initializedBitmap = value;
        }
        public bool HasImage => initializedBitmap != null;
        internal bool HasImageMetadata => imgData != null;
        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);

        public void SetImage(Metadata imgData, Vector2 scale)
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
            jImage = new();
            var colors = CBit.PixelArrayFromBitmap(Image);
            SetImage(colors);
        }
        public void SetImage(Vector2 size, byte[] data)
        {
            jImage = new(size, data);
        }
        public void SetImage(Pixel[,] colors)
        {
            jImage = new(colors);
        }
        public void SetImage(Pixel color)
        {
            jImage = new(CBit.SolidColorSquare(scale, color));
        }

        public Pixel GetPixel(int x, int y) => jImage.GetPixel(x, y);
        public void SetPixel(int x, int y, Pixel pixel) => jImage?.SetPixel(x, y, pixel);

        public JImage GetImage()
        {
            return jImage; 
        }

        public byte[] Data
        {
            get => jImage.data;
        }

        internal int Width
        {
            get => jImage.width; 
        }

        internal int Height
        {
            get => jImage.height; 
        }
    }
}
