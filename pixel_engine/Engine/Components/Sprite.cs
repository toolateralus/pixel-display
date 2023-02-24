using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
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
        [JsonProperty] private Vec2Int colorDataSize = new(1,1);
        [JsonProperty] public float camDistance = 1;
        [JsonProperty] public Texture texture;
        [JsonProperty] public SpriteType Type = SpriteType.SolidColor;
        [JsonProperty] public bool IsReadOnly = false;
        
        [Field][JsonProperty] public bool lit = false;
        [Field][JsonProperty] public Color color = Color.White;

        public bool dirty = true;
        internal protected bool selected_by_editor;

        private Color[,]? cached_colors = null;
        private Color[,]? lightmap; 
        private Color[,] LitColorData
        {
            get 
            {
                var light = GetFirstLight();

                if (light is null)
                    return _colors ?? throw new NullReferenceException(nameof(_colors));

                int X = _colors.GetLength(0);
                int Y = _colors.GetLength(1);

                if (lightmap is null || lightmap.Length != _colors.Length)
                    lightmap = new Color[X, Y];

                if (parent.TryGetComponent<Collider>(out var col))
                {
                    lightmap = VertexLighting(col.Polygon, light.parent.Position, light.radius, light.color, Polygon.GetBoundingBox(col.Polygon.vertices));
                }
                else
                {
                    Polygon poly = new Polygon(GetVertices()).OffsetBy(parent.Position);
                    lightmap = VertexLighting(poly, light.parent.Position, light.radius, light.color, Polygon.GetBoundingBox(poly.vertices));
                }

                return lightmap; 
            }
        }
        internal Color[,] ColorData
        {
            get
            {
                if (lit)
                    return LitColorData;
                return _colors ?? throw new NullReferenceException(nameof(_colors));
            }

            set
            {
                if (!IsReadOnly)
                {
                    _colors = value ?? throw new ArgumentNullException(nameof(value));
                    colorDataSize = new(_colors.GetLength(0), _colors.GetLength(1));
                }
            }
        }
        private Color[,] _colors = new Color[1,1];

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
        
        public Sprite()
        {
            
        }
        public Sprite(int x, int y) : this()
        {
            size = new(x, y);
            
        }
        
        public Vec2 ViewportToColorPos(Vec2 spriteViewport) => ((spriteViewport + viewportOffset) * viewportScale).Wrapped(Vec2.one) * colorDataSize;
        internal Vec2 GlobalToViewport(Vec2 global) => (global - parent.Position) / size.GetDivideSafe();
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
        public static bool PointInPolygon(Vec2 point, Vec2[] vertices)
        {
            int i, j = vertices.Length - 1;
            bool c = false;
            for (i = 0; i < vertices.Length; i++)
            {
                if (((vertices[i].y > point.y) != (vertices[j].y > point.y)) &&
                    (point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x))
                {
                    c = !c;
                }
                j = i;
            }
            return c;
        }

        public Light GetFirstLight()
        {
            var lights = Runtime.Current.GetStage().GetAllComponents<Light>();
            if (!lights.Any())
                return null; 
            return lights.First();
        }
        public void LightingPerPixel(Light light)
        {
            for (int x = 0; x < ColorData.GetLength(0); x++)
            {
                for (int y = 0; y < ColorData.GetLength(1); y++)
                {
                    Vec2 pixelPosition = new Vec2(parent.Position.x, parent.Position.y);

                    float distance = Vec2.Distance(pixelPosition, light.parent.Position);

                    float brightness = light.brightness / (distance * distance);

                    System.Drawing.Color originalColor = ColorData[x, y];

                    float newR = originalColor.R * brightness;
                    float newG = originalColor.G * brightness;
                    float newB = originalColor.B * brightness;

                    newR = Math.Max(0, Math.Min(255, newR));
                    newG = Math.Max(0, Math.Min(255, newG));
                    newB = Math.Max(0, Math.Min(255, newB));

                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(255, (int)newR, (int)newG, (int)newB);
                    ColorData[x, y] = newColor;
                }
            }
        }
        Color[,] VertexLighting(Polygon poly, Vec2 lightPosition, float lightRadius, Color lightColor, BoundingBox2D bounds)
        {
            // Get the vertices of the polygon
            Vec2[] vertices = poly.vertices;
            
            int vertexCount = vertices.Length;
            
            Color[,] colors = new Color[_colors.GetLength(0), _colors.GetLength(1)]; 
            
            int minY = (int)bounds.min.y;
            int maxY = (int)bounds.max.y;

            int minX = (int)bounds.min.x;
            int maxX = (int)bounds.max.x;

            for (int y = minY; y < maxY -1; y++)
                for (int x = minX; x < maxX -1; x++)

                    if (PointInPolygon(new Vec2(x, y), vertices))
                    {
                        float distance = Vec2.Distance(new Vec2(x, y), lightPosition);
                        float lightAmount = 1f - Math.Clamp(distance / lightRadius, 0,1);
                        int _x = x - minX;
                        int _y = y - minY;

                        Color existingColor = _colors[_x, _y];
                        Color blendedColor = ExtensionMethods.Lerp(existingColor, lightColor, lightAmount);
                        colors[x - minX, y - minY] = blendedColor;
                    }
            return colors; 
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
        /// <summary>
        /// caches the current color data of the sprite and sets every pixel in the color data to the one passed in.
        /// </summary>
        /// <param name="borderColor"></param>
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
    }
}
