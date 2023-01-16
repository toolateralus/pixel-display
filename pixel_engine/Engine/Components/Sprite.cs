using System;
using System.Drawing;
using System.Windows.Input;
using Newtonsoft.Json;
using pixel_renderer.Scripts;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public enum SpriteType { SolidColor, Image, Custom};
    public class Sprite : Component
    {
        [JsonProperty] public Vec2 size = Vec2.one * 16;
        [JsonProperty] public float camDistance = 1;
        [JsonProperty] public bool isCollider = false;
        [JsonProperty] public Texture texture = new(new Vec2(24, 24), Player.test_image_data);
        [JsonProperty] public Color Color
        {
            get
            { 
                Color? color = texture?.color ?? Color.White;
                return (Color)color; 
            }
            set => texture.color = value;
        }
        [JsonProperty] public SpriteType Type = SpriteType.Image;

        public bool dirty = true;

        private Color[,]? cached_colors = null;
        internal Color[,] ColorData 
        {
            get
            {
                if (!dirty) 
                    return _colors;
                    switch (Type)
                    {
                        case SpriteType.SolidColor:
                            _colors = SolidColorSquare(size, Color);
                            break;
                        case SpriteType.Image:
                            if (texture is null)
                                _colors = SolidColorSquare(size, Color);
                            else 
                                _colors = texture.GetColorArray();
                            break;
                        default:
                            throw new NotImplementedException("Custom Sprite render type not yet implemented");
                    }
                dirty = false; 
                return _colors; 
            }
            set => _colors = value;
        }
        private Color[,] _colors; 
        public override void Awake()
        {
            Input.RegisterAction(delegate {
                dirty = true; 
            }, Key.OemSemicolon);
            Input.RegisterAction(delegate {
                Color = JRandom.Color();
            }, Key.R);

        }
        public override void FixedUpdate(float delta)
        {
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
            Draw(size, colorData);
        }
        public void RestoreCachedColor(bool nullifyCache)
        {
            if (cached_colors == null) Randomize();
            Draw(size, cached_colors);
            if (nullifyCache) cached_colors = null;
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
                        colorData[x, y] = borderColor;
                }
            Draw(size, colorData);
        }
        public void Draw(Vec2 size, Color[,] color)
        {
            this.size = size;
            ColorData = color;
        }
        public void DrawSquare(Vec2 size, Color color)
        {
            this.size = size;
            ColorData = SolidColorSquare(size, color);
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
        public Sprite(){}
        public Sprite(int x, int y)
        {
            size = new(x, y);
            texture.scale.Set(size);
        }
    }
}
