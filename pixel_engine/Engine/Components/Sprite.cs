using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public enum SpriteType { SolidColor, Image, Custom};
    public class Sprite : Component
    {
        [JsonProperty] public Vec2 size = Vec2.one * 16;
        [JsonProperty] public Vec2 viewportScale = Vec2.one;
        [JsonProperty] public Vec2 viewportOffset = Vec2.zero;
        [JsonProperty] public float camDistance = 1;
        [JsonProperty] public Texture texture;
        [Field][JsonProperty] public Color color = Color.White;
        [JsonProperty] public SpriteType Type = SpriteType.SolidColor;

        public bool dirty = true;
        Vec2Int colorDataSize = new(1,1);

        private Color[,]? cached_colors = null;
        internal Color[,] ColorData
        {
            get => _colors ?? throw new NullReferenceException(nameof(_colors));
            set
            {
                if (!IsReadOnly)
                {
                    _colors = value ?? throw new ArgumentNullException(nameof(value));
                    colorDataSize = new(_colors.GetLength(0), _colors.GetLength(1));
                }
            }
        }

        public bool IsReadOnly = false;

        private void Refresh()
        {
            switch (Type)
            {
                case SpriteType.SolidColor:
                    _colors = CBit.SolidColorSquare(size, color);
                    break;
                case SpriteType.Image:
                    if (texture is null)
                        _colors = CBit.SolidColorSquare(size, color);
                    else
                        _colors = CBit.ColorArrayFromBitmap(texture.Image);
                    break;
               
                default: 
                    return; 
            }
            colorDataSize = new(_colors.GetLength(0), _colors.GetLength(1));
            dirty = false;
        }

        private Color[,] _colors = new Color[1,1];
        internal protected bool selected_by_editor;

        public override void Awake()
        {
            texture = new((Vec2Int)size, Player.PlayerSprite);
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
        public void RestoreCachedColor(bool nullifyCache, bool IsReadOnly = false)
        {
            this.IsReadOnly = IsReadOnly;
            if (cached_colors == null)
            {
                Bitmap bmp = new(Player.PlayerSprite.fullPath);
                cached_colors = CBit.ColorArrayFromBitmap(bmp);
                Runtime.Log("Sprite color cache was null upon returning to original color. Instantiating backup.");
            }

            Draw(size, cached_colors);
            if (nullifyCache) cached_colors = null;
        }
        /// <summary>
        /// caches the current color data of the sprite and sets every pixel in the color data to the one passed in.
        /// </summary>
        /// <param name="borderColor"></param>
        public void Highlight(Color borderColor, Vec2? widthIn = null, bool IsReadOnly = false)
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
            this.IsReadOnly = IsReadOnly;
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

        public Vec2 ViewportToColorPos(Vec2 spriteViewport) => ((spriteViewport + viewportOffset) * viewportScale).Wrapped(Vec2.one) * colorDataSize;
        internal Vec2 GlobalToViewport(Vec2 global) => (global - parent.Position) / size.GetDivideSafe();

        public Sprite()
        {
            
        }
        public Sprite(int x, int y) : this()
        {
            size = new(x, y);
            
        }
        public Vec2[] GetVertices()
        {
            Vec2 topLeft = Vec2.zero;
            Vec2 topRight = new(size.x, 0);
            Vec2 bottomRight = size;
            Vec2 bottomLeft = new(0, size.y);

            var vertices = new Vec2[]
            {
                    topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft,
            };

            return vertices;
        }
        public override void OnDrawShapes()
        {
            if (selected_by_editor)
            {
                Polygon mesh = new(GetVertices());
                int vertLength = mesh.vertices.Length;
                for (int i = 0; i < vertLength; i++)
                {
                    var nextIndex = (i + 1) % vertLength;
                    ShapeDrawer.DrawLine(mesh.vertices[i] + parent.Position, mesh.vertices[nextIndex] + parent.Position, Constants.EditorHighlightColor);
                }
            }
        }
    }
}
