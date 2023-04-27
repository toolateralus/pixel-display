using Newtonsoft.Json;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Numerics;

namespace Pixel
{
    public abstract class UIComponent : Component
    {
        internal protected bool dirty = true;

        [Field][JsonProperty] public Texture texture;
        [Field][JsonProperty] public bool lit = false;
        [Field][JsonProperty] public float drawOrder = 0f;
        [Field][JsonProperty] public bool IsReadOnly = false;
        [Field][JsonProperty] public float camDistance = 1;
        [Field][JsonProperty] public ImageType Type = ImageType.SolidColor;
        [Field][JsonProperty] public TextureFiltering filtering = TextureFiltering.Point;
        [Field][JsonProperty] public Color color = Color.Blue;
        [Field][JsonProperty] public Vector2 viewportPosition = Vector2.Zero;
        [Field][JsonProperty] public Vector2 viewportSize = Vector2.One;
        public readonly Vector2 half = Vector2.One * 0.5f;

        [Method]
        public void Refresh()
        {
            switch (Type)
            {
                case ImageType.SolidColor:
                    Color[,] colorArray = CBit.SolidColorSquare(Scale, color);
                    texture.SetImage(colorArray);
                    break;
                case ImageType.Image:
                    if (texture.imgData != null)
                        texture.SetImage(texture.imgData.Path);
                    break;
            }
            dirty = false;
        }
        /// <summary>
        /// re-draws the image *this is always called when marked dirty*
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        /// <summary>
        /// for any unmanaged resources that need to be disposed of, this is usually unneccesary.
        /// </summary>
        public abstract void Draw(RendererBase renderer);
        /// <summary>
        /// this is the default method for drawing.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="image"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// 
        public virtual void DrawImage(RendererBase renderer, JImage image)
        {
            var drawArea = GetSafeDrawArea(renderer);

            Vector2 framePos = drawArea.min;

            while (framePos.Y < drawArea.max.Y)
            {
                Vector2 screenViewport = framePos / renderer.Resolution;

                var localPos = ScreenToLocal(screenViewport);

                if (!RendererBase.IsWithinMaxExclusive(localPos.X, localPos.Y, -1, 1))
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                var colorPos = LocalToColorPosition(localPos);

                Color color = FilterPixel(image, colorPos);

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
        private Color FilterPixel(JImage image, Vector2 colorPos)
        {
            Color color;
            switch (filtering)
            {
                case TextureFiltering.Point:
                    color = image.GetPixel((int)colorPos.X, (int)colorPos.Y);
                    break;

                case TextureFiltering.Bilinear:


                    int left = (int)colorPos.X;
                    int top = (int)colorPos.Y;
                    int right = (int)((left + 1) % texture.size.X);
                    int bottom = (int)((top + 1) % texture.size.Y);

                    float xOffset = colorPos.X - left;
                    float yOffset = colorPos.Y - top;

                    Color tl, tr, bl, br;

                    GetAdjacentPixels(image, left, top, right, bottom, out tl, out tr, out bl, out br);

                    Color topPx = Color.Lerp(tl, tr, xOffset);
                    Color bottomPx = Color.Lerp(bl, br, xOffset);

                    color = Color.Lerp(topPx, bottomPx, yOffset);

                    break;

                default:
                    throw new NotImplementedException("Filtering not implemented");
            }

            return color;
        }
        private static void GetAdjacentPixels(JImage image, int left, int top, int right, int bottom, out Color tl, out Color tr, out Color bl, out Color br)
        {
            tl = image.GetPixel(left, top);
            tr = image.GetPixel(right, top);
            bl = image.GetPixel(left, bottom);
            br = image.GetPixel(right, bottom);
        }
        public override void Dispose()
        {
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
            var viewport = Polygon.Square(1);
            viewport.Transform(Transform);
            return viewport.vertices;
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
        public Vector2 LocalToColorPosition(Vector2 local) => ScreenToColorPos(LocalToScreen(local));
        public Vector2 ScreenToColorPos(Vector2 viewport)
        {
            viewport.X += 0.5f;
            viewport.Y += 0.5f;
            return viewport.Wrapped(Vector2.One) * Scale;
        }
        public Vector2 GlobalToScreen(Vector2 globalPos) => LocalToScreen(GlobalToLocal(globalPos));
        public Vector2 ScreenToGlobal(Vector2 normalizedScreenPos) => LocalToGlobal(ScreenToLocal(normalizedScreenPos));
        public Vector2 LocalToScreen(Vector2 local) =>
            ((local * viewportSize + viewportPosition) / 2) + half;
        public Vector2 ScreenToLocal(Vector2 screenViewport) =>
            (((screenViewport - half) * 2) - viewportPosition) / viewportSize.GetDivideSafe();
        #endregion
    }
}