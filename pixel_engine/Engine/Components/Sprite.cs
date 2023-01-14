using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Printing;
using System.Security.Policy;
using System.Windows.Controls;
using System.Windows.Documents;
using Newtonsoft.Json;
using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(string Name, Type fileType, string? UUID = null) : base(Name, fileType, UUID)
        {

        }
        public Texture(Metadata imageMetadata, Color? color = null, (Metadata, Bitmap)? mask = null )
        {
            m_image = new(imageMetadata, new(imageMetadata.fullPath)); 
            Mask = mask;
            this.color = color;
        }
        
        [JsonProperty] private Metadata imgData;
        [Field] [JsonProperty] public Color? color;

        public Bitmap? Image
        {
            get => m_image.Value.Item2;
            set { m_image = new(m_image.Value.Item1, value); }
        }
        public (Metadata, Bitmap)? m_image;
        [Field] public (Metadata, Bitmap)? Mask;
        
        public bool HasImage => Image != null;
        public bool HasImageMetadata => imgData != null; 

        public Color[,] GetColorArray()
        {
            if (Image is null)
                throw new NullReferenceException();
            Bitmap? copy = null;
            // clone the bitmap to prevent usage violations
            lock (Image)
                copy = (Bitmap)Image.Clone();


            Color[,] output = new Color[Image.Width, Image.Height];
            for (int i = 0; i < copy.Width; ++i)
                for (int j = 0; j < copy.Height; ++j)
                    output[i,j] = copy.GetPixel(i, j);
            return output; 
                    
        }
        

    }
    public enum SpriteType { SolidColor, Image, Custom};
    public class Sprite : Component
    {
        [JsonProperty] public Vec2 size = Vec2.one * 2;
        [JsonProperty] public float camDistance = 1;
        [JsonProperty] public bool isCollider = false;
        [JsonProperty] public Texture? texture = null;
        [JsonProperty] public Color Color
        {
            get
            { 
                Color? color = texture?.color ?? Color.White;
                return (Color)color; 
            }

        }
        [JsonProperty] public SpriteType Type = SpriteType.SolidColor;

        public bool dirty = false;

        private Color[,]? cached_colors = null;
        internal Color[,] ColorData 
        {
            get
            {
                switch (Type)
                {
                    case SpriteType.SolidColor:
                        _colors = SolidColorSquare(size, Color);
                        break;
                    case SpriteType.Image:
                        if (texture is null)
                            _colors = SolidColorSquare(size, Color);

                        _colors = texture.GetColorArray();
                        break;
                    case SpriteType.Custom:
                        throw new NotImplementedException("Custom Sprite render type not yet implemented");
                    default:
                        break;
                }
                return _colors; 
            }
            set => _colors = value;
        }
        private Color[,] _colors; 
        public override void Awake()
        {

        }
        public override void FixedUpdate(float delta)
        {
            if (dirty)
            {
                ColorData = texture.GetColorArray();
                dirty = false; 
            }
        }
        public void Randomize()
        {
            int x = (int)size.x;
            int y = (int)size.y;
            cached_colors = this.ColorData;
            var colorData = new Color[x, y];
            for (int j = 0; j < y; j++)
                for (int i = 0; i < x; i++)
                    colorData[i, j] = JRandom.Color();
            DrawSquare(size, colorData, isCollider);
        }
        public void RestoreCachedColor(bool nullifyCache)
        {
            if (cached_colors == null) Randomize();
            DrawSquare(size, cached_colors, isCollider);
            if (nullifyCache) cached_colors = null;
        }
        public void InitializeTestImage()
        {
            var imgData = new Metadata("test_sprite_image", Constants.WorkingRoot + Constants.ImagesDir + "\\home.bmp", ".bmp");
            texture.Image = new(imgData.fullPath);
            dirty = true;
        }

        /// <summary>
        /// caches the current color data of the sprite and sets every pixel in the color data to the one passed in.
        /// </summary>
        /// <param name="borderColor"></param>
        public void Highlight(Color borderColor, Vec2? widthIn = null)
        {
            cached_colors = this.ColorData;
            
            Vec2 width = widthIn ?? Vec2.one;
            
            int sizeX = (int)size.x;
            int sizeY = (int)size.y;

            var colorData = new Color[sizeX, sizeY];

            for (int x = 0; x< sizeX; ++x)
                for (int y = 0; y < sizeY; ++y)
                {
                    var pt = new Vec2(x, y);
                    if (!pt.IsWithinMaxExclusive(width, size - width))
                    {
                        colorData[x, y] = borderColor;
                    }
                }

            DrawSquare(size, colorData, isCollider);
        }
        
        public void DrawSquare(Vec2 size, Color[,] color, bool isCollider)
        {
            ColorData = color;
            this.size = size;
            this.isCollider = isCollider;
        }
        public void DrawSquare(Vec2 size, Color color, bool isCollider)
        {
            this.size = size;
            this.isCollider = isCollider;
            ColorData = new Color[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    ColorData[x, y] = color;
        }
        
        public static Bitmap SolidColorBitmap(Vec2 size, Color color)
        {
            int x = (int)size.x;
            int y = (int)size.y; 
            
            var bitmap = new Bitmap(x, y);

            for(int i = 0; i < x ; i++)
                for(int j = 0; j < x ; j++)
                    bitmap.SetPixel(i, j, color);
            return bitmap;
        }
        public static Color[,] SolidColorSquare(Vec2 size, Color color)
        {
            var colorData = new Color[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    colorData[x, y] = color;
            return colorData; 
        }



        public Sprite() => ColorData = new Color[0,0];
        public Sprite(int x, int y) => size = new(x, y);
        public Sprite(Vec2 size, Color color, bool isCollider) => DrawSquare(size, color, isCollider);
    }
}
