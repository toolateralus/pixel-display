﻿using System;
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
using pixel_renderer.Assets;
using pixel_renderer.Engine.Renderer;
using pixel_renderer.FileIO;
using Bitmap = System.Drawing.Bitmap;
using Pixel = System.Drawing.Color;

namespace pixel_renderer
{
    public enum SpriteType { SolidColor, Image, Custom};
    public enum TextureFiltering {Point, Bilinear};
    public class Sprite : Component
    {
        [JsonProperty] public Vec2 size = Vec2.one * 16;
        [JsonProperty] public Vec2 viewportScale = Vec2.one;
        [JsonProperty] public Vec2 viewportOffset = Vec2.zero;
        private Vec2Int colorDataSize = new(1, 1);
        public Vec2Int ColorDataSize => colorDataSize;
        [JsonProperty] public float camDistance = 1;
        [Field][JsonProperty] public Texture texture;
        [JsonProperty] [Field] public SpriteType Type = SpriteType.SolidColor;
        [JsonProperty] public bool IsReadOnly = false;
        [Field] [JsonProperty] public TextureFiltering textureFiltering = 0;
        [Field][JsonProperty] public bool lit = false;
        [Field][JsonProperty] public Pixel color = Color.White;
        public Sprite()
        {
            texture = new Texture((Vec2Int)size, Pixel.Red);
        }
        public Sprite(int x, int y) : this()
        {
            size = new(x, y);

        }
        internal protected bool dirty = true;
        internal protected bool selected_by_editor;
        

        private byte[]? lightmap; 
        private byte[] LitColorData
        {
            get 
            {
                var light = GetFirstLight();

                if (light is null)
                    return texture.jImage.data ?? throw new NullReferenceException(nameof(texture.jImage.data));

                int X = texture.jImage.data.GetLength(0);
                int Y = texture.jImage.data.GetLength(1);

                if (lightmap is null || lightmap.Length != texture.jImage.data.Length)
                    lightmap = new byte[X * Y * 4];
                
                // TODO: Fix vertex lighting.
                //if (parent.TryGetComponent<Collider>(out var col))
                    //lightmap = VertexLighting(col.Polygon, light.parent.Position, light.radius, light.color, Polygon.GetBoundingBox(col.Polygon.vertices));
                //else
                //{
                //    Polygon poly = new Polygon(GetVertices()).OffsetBy(parent.Position);
                    //lightmap = VertexLighting(poly, light.parent.Position, light.radius, light.color, Polygon.GetBoundingBox(poly.vertices));
                //}

                return lightmap; 
            }
        }
        internal byte[] ColorData
        {
            get
            {
                if (lit)
                    return LitColorData;
                if (texture.jImage.data is null) 
                    Refresh();
                if (texture.jImage.data is null)
                    throw new NullReferenceException(nameof(texture.jImage.data));

                return texture.jImage.data;
            }
            set
            {
                if (!IsReadOnly)
                {
                    texture.jImage.data = value ?? throw new ArgumentNullException(nameof(value));
                    colorDataSize = new(texture.jImage.width, texture.jImage.height);
                }
            }
        }

        [JsonProperty]
        [Field]
        public string textureName = "Table";
        [Method]
        public void TrySetTextureFromString()
        {
            if (AssetLibrary.FetchMeta(textureName) is Metadata meta)
                 texture = new(null, meta, (Vec2Int)size, meta.Name);
            Runtime.Log($"TrySetTextureFromString Called. Texture is null {texture == null} texName : {texture.Name}");
        }
        [Method]
        public void CycleSpriteType()
        {
            if ((int)Type + 1 > sizeof(SpriteType) - 2)
            {
                Type = 0;
                return;
            }
            Type = (SpriteType)((int)Type+ 1);

        }
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
        [Method]
        private void Refresh()
        {
            switch (Type)
            {
                case SpriteType.SolidColor:
                    Pixel[,] colorArray = CBit.SolidColorSquare(size, color);
                    texture.jImage.data = CBit.ByteArrayFromColorArray(colorArray);
                    texture.jImage.width = colorArray.GetLength(0);
                    texture.jImage.height = colorArray.GetLength(1);
                    break;
                case SpriteType.Image:
                    if (texture is null)
                    {
                        texture = new(null, new Metadata(Name, "", Constants.AssetsFileExtension), (Vec2Int)size);
                        Pixel[,] colorArray1 = CBit.SolidColorSquare(size, color);
                        texture.jImage.width = colorArray1.GetLength(0);
                        texture.jImage.height = colorArray1.GetLength(1);
                        texture.jImage.data = CBit.ByteArrayFromColorArray(colorArray1);
                    }
                    else
                    {
                        Pixel[,] colorArray1 = CBit.PixelArrayFromBitmap(texture.Image);
                        texture.jImage.width = colorArray1.GetLength(0);
                        texture.jImage.height = colorArray1.GetLength(1);
                        texture.jImage.data = CBit.ByteArrayFromColorArray(colorArray1);
                    }
                    break;
                default: throw new NotImplementedException();
            }
            colorDataSize = new(texture.jImage.width, texture.jImage.height);
            dirty = false;
        }
        public void Draw(Vec2 size, byte[] color)
        {
            this.size = size;
            ColorData = color;
        }
        public void DrawSquare(Vec2 size, Pixel color)
        {
            this.size = size;
            ColorData = CBit.ByteArrayFromColorArray(CBit.SolidColorSquare(size, color));
        }
        public void Highlight(Pixel borderColor, Vec2? widthIn = null, bool IsReadOnly = false)
        {
            
            Vec2 width = widthIn ?? Vec2.one;
            
            int sizeX = (int)size.x;
            int sizeY = (int)size.y;

            var colorData = new Pixel[sizeX, sizeY];

            for (int x = 0; x< sizeX; ++x)
                for (int y = 0; y < sizeY; ++y)
                {
                    var pt = new Vec2(x, y);
                    if (!pt.IsWithinMaxExclusive(width, size - width))
                        colorData[x, y] = borderColor;
                }
            Draw(size, CBit.ByteArrayFromColorArray(colorData));
            this.IsReadOnly = IsReadOnly;
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
        public Light? GetFirstLight()
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

                    Pixel originalPixel = texture.jImage.GetPixel(x, y);

                    float newR = originalPixel.r * brightness;
                    float newG = originalPixel.g * brightness;
                    float newB = originalPixel.b * brightness;

                    newR = Math.Max(0, Math.Min(255, newR));
                    newG = Math.Max(0, Math.Min(255, newG));
                    newB = Math.Max(0, Math.Min(255, newB));

                    Pixel newPixel = new(originalPixel.a, (byte)newR, (byte)newG, (byte)newB);
                    
                    texture.jImage.SetPixel(x, y, newPixel);
                }
            }
        }
        Pixel[,] VertexLighting(Polygon poly, Vec2 lightPosition, float lightRadius, Pixel lightColor, BoundingBox2D bounds)
        {
            // Get the vertices of the polygon
            Vec2[] vertices = poly.vertices;
            
            int vertexCount = vertices.Length;
            
            Pixel[,] colors = new Pixel[texture.jImage.data.GetLength(0), texture.jImage.data.GetLength(1)]; 
            
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
                        int _y = y - minY;
                        int _x = x - minX;

                        Pixel existingPixel = texture.jImage.GetPixel(_x, _y);
                        Pixel blendedPixel = Pixel.Lerp(existingPixel, lightColor, lightAmount);
                        colors[_x, _y] = blendedPixel;
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
            var colorData = new Pixel[x, y];

            for (int j = 0; j < y; j++)
                for (int i = 0; i < x; i++)
                    colorData[i, j] = JRandom.Color();

            Draw(size, CBit.ByteArrayFromColorArray(colorData));
        }
    }
}
