using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using pixel_renderer.ShapeDrawing;
using System.Linq;
using System;

namespace pixel_renderer
{
    public class UISprite : UIComponent
    {
        [JsonProperty] public Vector2 viewportScale = Vector2.One;
        [JsonProperty] public Vector2 viewportOffset = Vector2.Zero;
        [JsonProperty] private Vector2 colorDataSize = new(1, 1);
        public Vector2 ColorDataSize => colorDataSize;
        [JsonProperty]
        public float camDistance = 1;
        [JsonProperty]
        [Field]
        public Texture texture;
        [JsonProperty]
        [Field]
        public SpriteType Type = SpriteType.SolidColor;
        [JsonProperty]
        public bool IsReadOnly = false;
        [Field]
        [JsonProperty]
        public TextureFiltering textureFiltering = 0;
        [Field]
        [JsonProperty]
        public bool lit = false;
        [Field]
        [JsonProperty]
        public Pixel color = Pixel.Blue;

        internal protected bool dirty = true;
        internal protected bool selected_by_editor;
        private JImage lightmap;
        private JImage LitColorData
        {
            get
            {
                var light = GetFirstLight();

                var data = texture?.GetImage();

                if (light is null)
                    return data ?? throw new NullReferenceException(nameof(data));

                if (node.TryGetComponent<Collider>(out var col))
                {
                    Pixel[,] colors = VertexLighting(col.Polygon, light.node.Position, light.radius, light.color, col.BoundingBox);
                    lightmap = new(colors);
                }
                else
                {
                    Polygon poly = new Polygon(GetCorners()).OffsetBy(node.Position);
                    Pixel[,] colors = VertexLighting(col.Polygon, light.node.Position, light.radius, light.color, col.BoundingBox);
                    lightmap = new(colors);
                }
                return lightmap;
            }
        }
        internal JImage ColorData
        {
            get
            {
                if (lit)
                    return LitColorData;

                if (texture is null || texture.Data is null)
                    Refresh();

                if (texture is null || texture.Data is null)
                    throw new NullReferenceException(nameof(texture.Data));

                return texture.GetImage();
            }

        }
        [Method]
        public void CycleSpriteType()
        {
            if ((int)Type + 1 > sizeof(SpriteType) - 2)
            {
                Type = 0;
                return;
            }
            Type = (SpriteType)((int)Type + 1);

        }
        [Method]
        private void Refresh()
        {
            switch (Type)
            {
                case SpriteType.SolidColor:
                    Pixel[,] colorArray = CBit.SolidColorSquare(Vector2.One, color);
                    texture.SetImage(colorArray);
                    break;
                case SpriteType.Image:
                    if (texture is null)
                    {
                        texture = new(null, new Metadata(Name, "", Constants.AssetsFileExtension), Vector2.One);
                        Pixel[,] colorArray1 = CBit.SolidColorSquare(Vector2.One, color);
                        texture.SetImage(colorArray1);
                    }
                    else
                    {
                        Pixel[,] colorArray1 = CBit.PixelFromBitmap(texture.Image);
                        texture.SetImage(colorArray1);
                    }
                    break;
                default: throw new NotImplementedException();
            }
            colorDataSize = new(texture.Width, texture.Height);
            dirty = false;
        }

        public override void Awake()
        {
            texture = new(Vector2.One, Player.PlayerSprite);
            Refresh();

        }
        public override void FixedUpdate(float delta)
        {
            if (dirty)
                Refresh();
        }

        public UISprite()
        {
            texture = new Texture(Vector2.One, Pixel.Red);
        }
        public UISprite(int x, int y) : this()
        {
            Scale = new(x, y);
        }

        internal void SetImage(Pixel[,] colors) => texture.SetImage(colors);
        public void SetImage(Vector2 size, byte[] color)
        {
            texture.SetImage(size, color);
        }
        public void SetImage(JImage? image)
        {
            if (image is not null)
            {
                Vector2 size = new(image.width, image.height);
                SetImage(size, image.data);
            }
        }
        
        public Vector2 ViewportToColorPos(Vector2 spriteViewport) => ((spriteViewport + viewportOffset) * viewportScale).Wrapped(Vector2.One) * colorDataSize;
        internal Vector2 GlobalToViewport(Vector2 global)
        {
            return (global - Position) / Scale;
        }
       
        public void LightingPerPixel(Light light)
        {
            for (int x = 0; x < ColorData.width; x++)
            {
                for (int y = 0; y < ColorData.height; y++)
                {
                    Vector2 pixelPosition = new Vector2(node.Position.X, node.Position.Y);

                    float distance = Vector2.Distance(pixelPosition, light.node.Position);

                    float brightness = light.brightness / (distance * distance);

                    Pixel originalPixel = texture.GetPixel(x, y);

                    float newR = originalPixel.r * brightness;
                    float newG = originalPixel.g * brightness;
                    float newB = originalPixel.b * brightness;

                    newR = Math.Max(0, Math.Min(255, newR));
                    newG = Math.Max(0, Math.Min(255, newG));
                    newB = Math.Max(0, Math.Min(255, newB));

                    Pixel newPixel = new(originalPixel.a, (byte)newR, (byte)newG, (byte)newB);

                    texture.SetPixel(x, y, newPixel);
                }
            }
        }
        Pixel[,] VertexLighting(Polygon poly, Vector2 lightPosition, float lightRadius, Pixel lightColor, BoundingBox2D bounds)
        {
            // Get the vertices of the polygon
            Vector2[] vertices = poly.vertices;

            int vertexCount = vertices.Length;

            Pixel[,] colors = new Pixel[texture.Width, texture.Height];

            int minY = (int)bounds.min.Y;
            int maxY = (int)bounds.max.Y;

            int minX = (int)bounds.min.X;
            int maxX = (int)bounds.max.X;

            for (int y = minY; y < maxY - 1; y++)
                for (int x = minX; x < maxX - 1; x++)

                    if (PointInPolygon(new Vector2(x, y), vertices))
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), lightPosition);
                        float lightAmount = 1f - Math.Clamp(distance / lightRadius, 0, 1);
                        int _y = y - minY;
                        int _x = x - minX;

                        Pixel existingPixel = texture.GetPixel(_x, _y);
                        Pixel blendedPixel = Pixel.Lerp(existingPixel, lightColor, lightAmount);
                        colors[_x, _y] = blendedPixel;
                    }
            return colors;
        }
        
        public Light? GetFirstLight()
        {
            var lights = Runtime.Current.GetStage().GetAllComponents<Light>();
            if (!lights.Any())
                return null;
            return lights.First();
        }
        public Vector2[] GetCorners()
        {
            Vector2 topLeft = Vector2.Transform(new Vector2(-0.5f, -0.5f), Transform);
            Vector2 topRight = Vector2.Transform(new Vector2(0.5f, -0.5f), Transform);
            Vector2 bottomRight = Vector2.Transform(new Vector2(0.5f, 0.5f), Transform);
            Vector2 bottomLeft = Vector2.Transform(new Vector2(-0.5f, 0.5f), Transform);

            var vertices = new Vector2[]
            {
                topLeft,
                topRight,
                bottomRight,
                bottomLeft,
            };

            return vertices;
        }

        public static bool PointInPolygon(Vector2 point, Vector2[] vertices)
        {
            int i, j = vertices.Length - 1;
            bool c = false;
            for (i = 0; i < vertices.Length; i++)
            {
                if (((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y)) &&
                    (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                {
                    c = !c;
                }
                j = i;
            }
            return c;
        }
        public override void OnDrawShapes()
        {
            if (selected_by_editor)
            {
                Polygon mesh = new(GetCorners());
                int vertLength = mesh.vertices.Length;
                for (int i = 0; i < vertLength; i++)
                {
                    var nextIndex = (i + 1) % vertLength;
                    ShapeDrawer.DrawLine(mesh.vertices[i], mesh.vertices[nextIndex], Constants.EditorHighlightColor);
                }
            }
        }

        public override void Draw(ref Vector2 resolution, ref byte[] frame)
        {
            Runtime.GetRenderingData(out var host, out var info, out var renderer, out var baseImg);
            Position = Vector2.Zero; 


        }
    }

    public class Text : UIComponent
    {
        public Dictionary<char, (JImage image, Vector2 scale)> font = new();
        public Dictionary<Sprite, char> nodes = new(); 
        /// <summary>
        /// the bounding box of the text element
        /// </summary>

        public BoundingBox2D bounds;
        const string alphabet = "abcdefghijklmnopqrstuvwxyz"; 

        public override void Awake()
        {
            for (int i = 0; i < 3; ++i)
            {
                var meta = AssetLibrary.FetchMetaRelative($"\\Assets\\Fonts\\font{i}.bmp");
                if (meta != null)
                {
                    Bitmap bmp = new(meta.Path);
                    JImage image = new(bmp);
                    
                    Node node = Rigidbody.Standard("Font Test Node.");


                    node.Transform = Matrix3x2.CreateScale(35);
                    if (!node.TryGetComponent(out Sprite sprite)) 
                        continue;

                    sprite.texture.SetImage(image);

                    var scale = sprite.texture.scale; 


                    font.Add(alphabet[i], (image, scale));
                    nodes.Add(sprite, alphabet[i]);
                }
            }
        }
        public override void Update()
        {
            foreach (var node in nodes.Keys)
                node.Position = Position + Vector2.One * 15; 
        }

        public override void Draw(ref Vector2 resolution, ref byte[] frame)
        {
            throw new NotImplementedException();
        }
    }
}
