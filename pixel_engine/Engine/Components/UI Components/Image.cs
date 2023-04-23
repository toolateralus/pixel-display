﻿using pixel_renderer.Assets;
using System.Numerics;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using pixel_renderer.ShapeDrawing;
using System.Linq;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class Image : UIComponent
    {
        
        internal protected bool selected_by_editor;
        private JImage lightmap;

        [Field] [JsonProperty]
        private string textureName = "Assets\\other\\ball.bmp";
        public override void Dispose()
        {
        }
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
        public override void Awake()
        {
            SetFileAsTexture();
            Refresh();
        }
        public override void FixedUpdate(float delta)
        {
            if (dirty)
                Refresh();
        }
        public override void Draw(RendererBase renderer)
        {
            var image = texture.GetImage();
            DrawImage(renderer, image);
        }
        public Image()
        {
            texture = new Texture(Vector2.One, Pixel.Red);
        }   
        [Method] public void CycleSpriteType()
        {
            if ((int)Type + 1 > sizeof(ImageType) - 2)
            {
                Type = 0;
                return;
            }
            Type = (ImageType)((int)Type + 1);

        }
        [Method] public async Task SetFileAsTexture()
        {
            EditorEvent e = new("$nolog_get_selected_meta");
            object? asset = null;
            e.action = (e) => { asset = e.First(); };
            Runtime.RaiseInspectorEvent(e);
            float time = 0;

            while (!e.processed && time < 1500)
            {
                if (asset != null && asset is Metadata meta)
                {
                    texture.SetImage(meta.Path);
                    return;
                }
                time += 15f;
                await Task.Delay(15);
            }
        }
        internal void SetImage(Pixel[,] colors) => texture.SetImage(colors);
        public void SetImage(Vector2 size, byte[] color)
        {
            texture.SetImage(size, color);
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
        public static Node Standard()
        {
            Node node = new("UI Element");
            var img = node.AddComponent<Image>();

            img.Scale = Constants.DefaultNodeScale;
            img.Type = ImageType.Image; 
            img.SetFileAsTexture();
            img.Refresh();

            return node; 
        }
        #region Lighting
        public Light? GetFirstLight()
        {
            var lights = Runtime.Current.GetStage().GetAllComponents<Light>();
            if (!lights.Any())
                return null;
            return lights.First();
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
        #endregion
       
    }
}
