using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

            LightingPerPixel(); 
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
        public void LightingPerPixel()
        {
            var lights = Runtime.Current.GetStage().GetAllComponents<Light>();
            if (!lights.Any()) 
                return;

            Light light = lights.First();
            var lightPosition = light.parent.Position;


            for (int x = 0; x < ColorData.GetLength(0); x++)
            {
                for (int y = 0; y < ColorData.GetLength(1); y++)
                {
                    Vec2 pixelPosition = new Vec2(parent.Position.x, parent.Position.y);

                    float distance = Vec2.Distance(pixelPosition, lightPosition);

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
        void VertexLighting(Polygon poly, Vec2 lightPosition, float lightRadius, Color lightColor, BoundingBox2D bounds)
        {
            // Get the vertices of the polygon
            Vec2[] vertices = poly.vertices;
            int vertexCount = vertices.Length;

            // Iterate over each horizontal row of the bounding box
            for (int y = (int)bounds.min.y; y < bounds.max.y; y++)
            {
                // Iterate over each column of the bounding box
                for (int x = (int)bounds.min.x; x < bounds.max.x; x++)
                {
                    // Check if the current point is inside the polygon
                    if (PointInPolygon(new Vec2(x, y), vertices))
                    {
                        // Calculate the distance between the current point and the light position
                        float distance = Vec2.Distance(new Vec2(x, y), lightPosition);

                        // Calculate the amount of light that reaches the current point
                        float lightAmount = 1f - Math.Clamp(distance / lightRadius, 0,1);

                        // Lerp the light color and the existing color at the current point
                        Color existingColor = _colors[x - (int)bounds.min.x, y - (int)bounds.min.y];
                        Color blendedColor = ExtensionMethods.Lerp(existingColor, lightColor, lightAmount);

                        // Set the color at the current point
                        _colors[x - (int)bounds.min.x, y - (int)bounds.min.y] = blendedColor;
                    }
                }
            }
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
