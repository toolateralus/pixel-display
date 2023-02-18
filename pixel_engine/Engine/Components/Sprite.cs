using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Input;
using Newtonsoft.Json;
using pixel_renderer.Scripts;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public enum SpriteType { SolidColor, Image, Custom};
    public class Sprite : Component
    {
        [JsonProperty] public Vec2 size = Vec2.one * 16;
        [JsonProperty] public Vec2 viewportOffset = Vec2.zero;
        [JsonProperty] public float camDistance = 1;
        [JsonProperty] public Texture texture;
        [JsonProperty] public Color Color
        {
            get
            {
                Color? color = texture?.color ?? Color.White;
                return (Color)color;
            }
            set
            {
                if (texture is null)
                    return; 

                texture.color = value; 
            }
        }
        [JsonProperty] public SpriteType Type = SpriteType.SolidColor;

        public bool dirty = true;
        Vec2Int colorDataSize = new(1,1);

        private Color[,]? cached_colors = null;
        internal Color[,] ColorData
        {
            get => _colors;
            set
            {
                _colors = value;
                colorDataSize = new(_colors.GetLength(0), _colors.GetLength(1));
            }
        }

        private void Refresh()
        {
            switch (Type)
            {
                case SpriteType.SolidColor:
                    _colors = CBit.SolidColorSquare(size, Color);
                    break;
                case SpriteType.Image:
                    if (texture is null)
                        _colors = CBit.SolidColorSquare(size, Color);
                    else
                        _colors = CBit.ColorArrayFromBitmap(texture.Image);
                    break;
               
                default: 
                    return; 
            }
            colorDataSize = new(_colors.GetLength(0), _colors.GetLength(1));
            dirty = false;
        }

        private Color[,] _colors; 
       
        public override void Awake()
        {
            texture = new((Vec2Int)size, Player.test_image_data);
            Refresh();

        }
        public override void FixedUpdate(float delta)
        {
            if (dirty)
                Refresh();
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
            ColorData = CBit.SolidColorSquare(size, color);
        }

        

        public Vec2 ViewportToColorPos(Vec2 spriteViewport) => (spriteViewport + viewportOffset).Wrapped(Vec2.one) * colorDataSize;
        internal Vec2 GlobalToViewport(Vec2 global) => (global - parent.Position) / size.GetDivideSafe();


        internal void Highlight(object editorHighlightColor)
        {
            throw new NotImplementedException();
        }

        public Sprite()
        {
            
        }
        public Sprite(int x, int y) : this()
        {
            size = new(x, y);
            
        }
    }
}
