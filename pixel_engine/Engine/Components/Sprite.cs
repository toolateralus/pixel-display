﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using pixel_renderer.ShapeDrawing;
using Bitmap = System.Drawing.Bitmap;
using Pixel = System.Drawing.Color;

namespace pixel_renderer
{
    public enum SpriteType { SolidColor, Image, Custom};
    public enum TextureFiltering { Point, Bilinear }
    public class Sprite : Component
    {
        internal protected bool IsDirty = true;
        internal protected bool selected_by_editor;

        private JImage lightmap;
        private void ApplyLighting()
        {
            var light = GetFirstLight();
            var data = texture?.GetImage();
            if (light is null)
                return;
            if (!node.TryGetComponent<Collider>(out var col))
                return; 

            if (data is not null)
            {
                Pixel[,] colors = VertexLighting(col.Polygon, light.node.Position, light.radius, light.color);
                lightmap = new(colors);
            }
            texture?.SetImage(lightmap);
        }
      

        [JsonProperty] protected Vector2 colorDataSize = new(16, 16);
        [Field] [JsonProperty] public Vector2 viewportScale = new(1, 1);
        [Field] [JsonProperty] public Vector2 viewportOffset = new(0.0f, 0.0f);
        [Field] [JsonProperty] public float camDistance = 1;
        [Field] [JsonProperty] public Texture texture;
        [Field] [JsonProperty] public SpriteType Type = SpriteType.SolidColor;
        [Field] [JsonProperty] public TextureFiltering textureFiltering = 0;
        [Field] [JsonProperty] public bool lit = false;
        [Field] [JsonProperty] public Pixel color = Pixel.Blue;
        [Field] [JsonProperty] public float drawOrder = 0f;
        [Field] [JsonProperty] public bool IsReadOnly = false;
        [Field] [JsonProperty] public TextureFiltering filtering = TextureFiltering.Point;
        [Field] [JsonProperty] public string textureName = "Table";
        
        public Sprite()
        {
            texture = new Texture(Vector2.One, Pixel.Red);
        }
        public Sprite(int x, int y) : this()
        {
            Scale = new(x, y);
        }

        [Method]
        public void TrySetTextureFromString()
        {
            if (AssetLibrary.FetchMeta(textureName) is Metadata meta)
            {
                texture.SetImage(meta.Path);
                texture.Image = new(meta.Path);
            }

            Runtime.Log($"TrySetTextureFromString Called. Texture is null {texture == null} texName : {texture.Name}");
        }
        [Method]
        public void CycleSpriteType()
        {
            switch (Type)
            {
                case SpriteType.SolidColor:
                    Type = SpriteType.Image;
                    break;
                case SpriteType.Image:
                    Type = SpriteType.SolidColor;
                    break;
                case SpriteType.Custom:
                    Type = SpriteType.SolidColor;
                    break;
                default:
                    break;
            }

        }
        [Method]
        private void Refresh()
        {
            switch (Type)
            {
                case SpriteType.SolidColor:
                    Pixel[,] colorArray = CBit.SolidColorSquare(new(16,16), color);
                    texture.SetImage(colorArray);
                    break;
                case SpriteType.Image:
                    if (texture is null || texture.Image is null)
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
            IsDirty = false;
        }

        public override void Awake()
        {
            texture = new(Vector2.One, Player.PlayerSprite);
            Refresh();

        }
        public override void FixedUpdate(float delta)
        {
            if (IsDirty)
                Refresh();

            if (lit)
            {
                Refresh(); 
                ApplyLighting();
            }
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
                    ShapeDrawer.DrawLine(mesh.vertices[i], mesh.vertices[nextIndex], Runtime.Current.projectSettings.EditorHighlightColor);
                }
            }
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

        public void LightingPerPixel(Light light)
        {
            int x = 0, y = 0;

            PixelShader((e) => { },  getColor, getPosition, X, Y, OnIterationComplete, new object[] { colorDataSize.X, colorDataSize.Y});

            int Y() => y++; 
            int X() => x++; 

            void OnIterationComplete(JImage image) { }
            Vector2 getPosition() { return new(x, y);  }
            Pixel getColor() { return Pixel.White; };
        }
        Pixel[,] VertexLighting(Polygon poly, Vector2 lightPosition, float lightRadius, Pixel lightColor)
        {
            
            Pixel[,] colors = new Pixel[texture.Width, texture.Height]; 
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    {
                        float distance = Vector2.Distance(Position + new Vector2(x,y), lightPosition);
                        float lightAmount = 1f - Math.Clamp(distance / lightRadius, 0,1);
                        Pixel existingPixel = texture.GetPixel(x, y);
                        Pixel blendedPixel = Pixel.Lerp(existingPixel, lightColor, lightAmount);
                        colors[x, y] = blendedPixel;
                    }
            return colors; 
        }
        public virtual Pixel[,] PixelShader(Action<Pixel> colorOut, Func<Pixel> colorIn, Func<Vector2> position,  Func<int> indexerX, Func<int> indexerY, Action<JImage> onIteraton,  params object[] args)
        {
            
            int width = (int)args[0];
            int height = (int)args[1];
            Pixel[,] pixels = new Pixel[width, height];

            for (int x = 0; x < width; indexerX.Invoke())
                for (int y = 0; y < height; indexerY.Invoke())
                {
                    Vector2 pos = position.Invoke();
                    int _x = (int)pos.X; 
                    int _y = (int)pos.Y; 

                    var col = texture.GetPixel(_x, _y);
                    colorOut.Invoke(col);
                    texture.SetPixel(_x, _y, colorIn.Invoke());
                    onIteraton.Invoke(texture.GetImage());
                }

            return pixels; 
        }


        public Light? GetFirstLight()
        {
            var lights = Runtime.Current.GetStage().GetAllComponents<Light>();
            if (!lights.Any())
                return null; 
            return lights.First();
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
    }
}
