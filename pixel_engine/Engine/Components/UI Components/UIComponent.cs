using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;

namespace pixel_renderer
{
    public abstract class UIComponent : Component
    {
        internal protected bool dirty = true;
        public Vector2 ColorDataSize => colorDataSize;
        public Vector2 Size
        {
            get => new(Transform.M11, Transform.M22);
            set
            {
                Transform.M11 = value.X;
                Transform.M22 = value.Y;
            }
        }
        public Vector2 Center { get => Transform.Translation; set => Transform.Translation = value; }

        [JsonProperty] protected Vector2 colorDataSize = new(1, 1);
        [Field][JsonProperty] public Vector2 viewportScale = new(1, 1);
        [Field][JsonProperty] public Vector2 viewportOffset = new(0.0f, 0.0f);
        [Field][JsonProperty] public float camDistance = 1;
        [Field][JsonProperty] public Texture texture;
        [Field][JsonProperty] public SpriteType Type = SpriteType.SolidColor;
        [Field][JsonProperty] public TextureFiltering textureFiltering = 0;
        [Field][JsonProperty] public bool lit = false;
        [Field][JsonProperty] public Pixel color = Pixel.Blue;
        [Field][JsonProperty] public float drawOrder = 0f;
        [Field][JsonProperty] public bool IsReadOnly = false;
        [Field][JsonProperty] public TextureFiltering filtering = TextureFiltering.Point;

        public abstract void Draw(RendererBase renderer); 
        /// <summary>
        /// this is the default method for drawing.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="image"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void DrawImage(RendererBase renderer, JImage image)
        {
            BoundingBox2D drawArea = GetSafeDrawArea(renderer);

            Vector2 framePos = drawArea.min;

            while (framePos.Y < drawArea.max.Y)
            {

                Vector2 screenViewport = framePos / renderer.Resolution;

                var spriteLocal = ScreenViewportToLocal(screenViewport);

                if (!RendererBase.IsWithinMaxExclusive(spriteLocal.X, spriteLocal.Y, -1, 1))
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                var colorPos = LocalToColorPosition(spriteLocal);

                Pixel color = new Pixel();

                switch (filtering)
                {
                    case TextureFiltering.Point:
                        color = image.GetPixel((int)colorPos.X, (int)colorPos.Y);
                        break;
                    case TextureFiltering.Bilinear:
                        Vector2 colorSize = colorDataSize;

                        int left = (int)colorPos.X;
                        int top = (int)colorPos.Y;
                        int right = (int)((left + 1) % colorSize.X);
                        int bottom = (int)((top + 1) % colorSize.Y);

                        float xOffset = colorPos.X - left;
                        float yOffset = colorPos.Y - top;

                        Pixel topJPixel = Pixel.Lerp(image.GetPixel(left, top), image.GetPixel(right, top), xOffset);
                        Pixel botJPixel = Pixel.Lerp(image.GetPixel(left, bottom), image.GetPixel(right, bottom), xOffset);
                        color = Pixel.Lerp(topJPixel, botJPixel, yOffset);
                        break;
                    default:
                        throw new NotImplementedException("Filtering not implemented");
                }

                if (color.a == 0)
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                renderer.WriteColorToFrame(ref color, ref framePos);

                framePos.X++;

                if (framePos.X >= drawArea.max.X)
                {
                    framePos.X = drawArea.min.X;
                    framePos.Y++;
                }

            }
        }
        /// <summary>
        /// re-draws the image *this is always called when marked dirty*
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        [Method]
        internal protected void Refresh()
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
        #region Coordinate Functions
        public BoundingBox2D GetSafeDrawArea(RendererBase renderer)
        {
            var drawArea = new BoundingBox2D(GetCorners());
            drawArea.min = Vector2.Max(Vector2.Zero, drawArea.min);
            drawArea.max = Vector2.Min(renderer.Resolution, drawArea.max);
            return drawArea;
        }
        public Vector2[] GetCorners()
        {
            return new Vector2[]
            {
                Vector2.Transform(new Vector2(-0.5f, -0.5f), Transform), // Top Left
                Vector2.Transform(new Vector2(0.5f, -0.5f), Transform), // Top Right
                Vector2.Transform(new Vector2(0.5f, 0.5f), Transform), // Bottom Right
                Vector2.Transform(new Vector2(-0.5f, 0.5f), Transform), // Bottom Left
            };
        }
        internal Vector2 GlobalToLocal(Vector2 global)
        {
            Matrix3x2.Invert(Transform, out var inverted);
            return Vector2.Transform(global, inverted);
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
        public Vector2 LocalToGlobal(Vector2 local) => Vector2.Transform(local, Transform);
        public Vector2 ViewportToColorPos(Vector2 spriteViewport) => ((spriteViewport + viewportOffset) * viewportScale).Wrapped(Vector2.One) * colorDataSize;
        internal Vector2 GlobalToViewport(Vector2 global)
        {
            return (global - Position) / Scale;
        }
        public Vector2 ScreenViewportToLocal(Vector2 screenViewport)
        {
            viewportScale.MakeDivideSafe();
            return (screenViewport - viewportOffset) / viewportScale;
        }
        public Vector2 LocalToViewport(Vector2 local) => (local + viewportOffset) * viewportScale;
        public Vector2 LocalToColorPosition(Vector2 local) => ViewportToColorPosition(LocalToViewport(local));
        public Vector2 ViewportToColorPosition(Vector2 viewport)
        {
            viewport.X += 0.5f;
            viewport.Y += 0.5f;
            return viewport.Wrapped(Vector2.One) * colorDataSize;
        }
        #endregion
        /// <summary>
        /// for any unmanaged resources that need to be disposed of, this is usually unneccesary.
        /// </summary>
        public virtual void Dispose()
        {
            throw new NotImplementedException(); 
        }

    }
}