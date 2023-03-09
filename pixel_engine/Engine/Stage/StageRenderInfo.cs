using pixel_renderer;
using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;

namespace pixel_renderer
{
    public class StageRenderInfo
    {
        public List<SpriteInfo> spriteInfos = new();
        public List<CameraInfo> cameraInfos = new();

        public StageRenderInfo(Stage stage)
        {
           Refresh(stage);
        }
        public void Refresh(Stage stage)
        {
            UpdateList(spriteInfos, stage.GetSprites());
            UpdateList(cameraInfos, stage.GetAllComponents<Camera>());
        }

        private void UpdateList<T>(List<T> infoList, IEnumerable<object> objects) where T : ViewpoortInfoObject, new()
        {
            int objectCount = objects.Count();
            int infoCount = infoList.Count;
            if (objectCount != infoCount)
            {
                for (int i = infoCount; i < objectCount; ++i)
                    infoList.Add(new());
                for (int i = infoCount; i > objectCount; --i)
                    infoList.RemoveAt(0);
            }
            for (int i = 0; i < objects.Count(); ++i)
            {
                infoList[i].Set(objects.ElementAt(i));
            }
        }
    }
    public abstract class ViewpoortInfoObject
    {
        readonly Vector2 half = new(0.5f, 0.5f);
        public Matrix3x2 Transform;
        public Vector2 viewportOffset = new();
        public Vector2 viewportScale = new();
        public Vector2 Position { get => Transform.Translation; set => Transform.Translation = value; }
        public Vector2[] GetCorners()
        {
            var viewport = Polygon.UnitSquare();
            viewport.Transform(Transform);
            return viewport.vertices;
        }
        public Vector2 LocalToViewport(Vector2 local) => (local + viewportOffset) * viewportScale;
        public Vector2 GlobalToScreen(Vector2 globalPos) => LocalToScreen(GlobalToLocal(globalPos));
        public Vector2 ScreenToGlobal(Vector2 normalizedScreenPos) => LocalToGlobal(ScreenToLocal(normalizedScreenPos));
        public Vector2 LocalToScreen(Vector2 local) =>
            ((local * viewportScale + viewportOffset) / 2) + half;
        public Vector2 ScreenToLocal(Vector2 screenViewport) =>
            (((screenViewport - half) * 2) - viewportOffset) / viewportScale.GetDivideSafe();
        public Vector2 LocalToGlobal(Vector2 local) => local.Transformed(Transform);
        internal Vector2 GlobalToLocal(Vector2 global) => global.Transformed(Transform.Inverted());

        public abstract void Set(object? refObject);
    }
    public class SpriteInfo : ViewpoortInfoObject
    {
        public Vector2 scale;
        public float camDistance = new();
        public TextureFiltering filtering = new();
        public JImage image = new();
        public Vector2 colorDataSize;
        public override void Set(object? refObject)
        {
            if (refObject is not Sprite sprite)
                throw new ArgumentException($"Must pass in object of type {nameof(Sprite)}");
            viewportOffset = sprite.viewportOffset;
            viewportScale = sprite.viewportScale.GetDivideSafe();
            camDistance = sprite.camDistance;
            
            image = sprite.texture.GetImage();
            colorDataSize = image.Size;
            filtering = sprite.textureFiltering;
            
            Transform = sprite.Transform;
            scale = sprite.Scale;
        }
        public Vector2 LocalToColorPosition(Vector2 local) => ViewportToColorPosition(LocalToViewport(local));
        public Vector2 ViewportToColorPosition(Vector2 viewport)
        {
            viewport.X += 0.5f;
            viewport.Y += 0.5f;
            return viewport.Wrapped(Vector2.One) * colorDataSize;
        }
        public void SetColorData(Vector2 size, byte[] data)
        {
            image = new(size, data);
            colorDataSize = new(size.X, size.Y);
        }

    }
    public class CameraInfo : ViewpoortInfoObject
    {
        public float[,] zBuffer = new float[0, 0];
        public override void Set(object? refObject)
        {
            if (refObject is not Camera cam)
                throw new ArgumentException($"Must pass in object of type {nameof(Camera)}");
            viewportOffset = cam.viewportPosition;
            viewportScale = cam.viewportSize.GetDivideSafe();
            Transform = cam.Transform;
            cam.camInfo = this;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Draw(RendererBase renderer)
        {
            if (renderer.Resolution.Y == 0 || renderer.Resolution.X == 0) return;

            if (zBuffer.GetLength(0) != renderer.Resolution.X || zBuffer.GetLength(1) != renderer.Resolution.Y)
                zBuffer = new float[(int)renderer.Resolution.X, (int)renderer.Resolution.Y];

            Array.Clear(zBuffer);

            DrawBaseImage(renderer.Resolution, renderer.baseImage, renderer);
            DrawSprites(Runtime.Current.GetStage().StageRenderInfo, renderer.Resolution, renderer);
            DrawGraphics(renderer.Resolution, renderer);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawGraphics(Vector2 resolution, RendererBase renderer)
        {
            Vector2 framePos = new Vector2();
            foreach (pixel_renderer.ShapeDrawing.Circle circle in ShapeDrawer.Circles)
            {
                float sqrtOfHalf = MathF.Sqrt(0.5f);
                Vector2 radius = circle.center + new Vector2(circle.radius, circle.radius);
                Vector2 centerPos = GlobalToScreen(circle.center) * resolution;
                Vector2 pixelRadius = GlobalToScreen(radius) * resolution - centerPos;
                Vector2 quaterArc = pixelRadius * sqrtOfHalf;
                int quarterArcAsInt = (int)quaterArc.X;
                for (int x = -quarterArcAsInt; x <= quarterArcAsInt; x++)
                {
                    float y = MathF.Cos(MathF.Asin(x / pixelRadius.X)) * pixelRadius.Y;
                    framePos.X = centerPos.X + x;
                    framePos.Y = centerPos.Y + y;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref circle.color, ref framePos);
                    framePos.Y = centerPos.Y - y;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref circle.color, ref framePos);
                }
                quarterArcAsInt = (int)quaterArc.Y;
                for (int y = -quarterArcAsInt; y <= quarterArcAsInt; y++)
                {
                    float x = MathF.Cos(MathF.Asin(y / pixelRadius.Y)) * pixelRadius.X;
                    framePos.Y = centerPos.Y + y;
                    framePos.X = centerPos.X + x;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref circle.color, ref framePos);
                    framePos.X = centerPos.X - x;
                    if (framePos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref circle.color, ref framePos);
                }
            }
            foreach (Line line in ShapeDrawer.Lines)
            {
                Vector2 startPos = GlobalToScreen(line.startPoint) * resolution;
                Vector2 endPos = GlobalToScreen(line.endPoint) * resolution;
                if (startPos == endPos)
                {
                    if (startPos.IsWithinMaxExclusive(Vector2.Zero, resolution))
                        renderer.WriteColorToFrame(ref line.color, ref startPos);
                    continue;
                }

                float xDiff = startPos.X - endPos.X;
                float yDiff = startPos.Y - endPos.Y;

                if (MathF.Abs(xDiff) > MathF.Abs(yDiff))
                {
                    float slope = yDiff / xDiff;
                    float yIntercept = startPos.Y - (slope * startPos.X);

                    int endX = (int)MathF.Min(MathF.Max(startPos.X, endPos.X), resolution.X);

                    for (int x = (int)MathF.Max(MathF.Min(startPos.X, endPos.X), 0); x < endX; x++)
                    {
                        framePos.X = x;
                        framePos.Y = slope * x + yIntercept;
                        if (framePos.Y < 0 || framePos.Y >= resolution.Y)
                            continue;
                        renderer.WriteColorToFrame(ref line.color, ref framePos);
                    }
                }
                else
                {
                    float slope = xDiff / yDiff;
                    float xIntercept = startPos.X - (slope * startPos.Y);

                    int endY = (int)MathF.Min(MathF.Max(startPos.Y, endPos.Y), resolution.Y);

                    for (int y = (int)MathF.Max(MathF.Min(startPos.Y, endPos.Y), 0); y < endY; y++)
                    {
                        framePos.Y = y;
                        framePos.X = slope * y + xIntercept;
                        if (framePos.X < 0 || framePos.X >= resolution.X)
                            continue;
                        renderer.WriteColorToFrame(ref line.color, ref framePos);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawSprites(StageRenderInfo renderInfo, Vector2 resolution, RendererBase renderer)
        {
            SpriteInfo sprite;
            BoundingBox2D drawArea = new();
            for (int i = 0; i < renderInfo.spriteInfos.Count; ++i)
            {
                sprite = renderInfo.spriteInfos[i];
                drawArea = new(sprite.GetCorners());
                drawArea.min = GlobalToLocal(drawArea.min);
                drawArea.max = GlobalToLocal(drawArea.max);
                if (drawArea.min.X >= 1 || drawArea.max.X <= -1 ||
                    drawArea.min.Y >= 1 || drawArea.max.Y <= -1)
                    continue;
                drawArea.min = LocalToScreen(drawArea.min) * resolution;
                drawArea.max = LocalToScreen(drawArea.max) * resolution;

                DrawTransparentSprite(sprite, drawArea, resolution, renderer);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawBaseImage(Vector2 resolution, JImage baseImage, RendererBase renderer)
        {
            SpriteInfo sprite = new();

            var stage = Runtime.Current.GetStage();
            Vector2 baseImageSize;

            if (stage != null)
                baseImageSize = stage.backgroundSize;
            else baseImageSize = new(16, 16);

            BoundingBox2D camBoundingBox = new(GetCorners());

            var scale = camBoundingBox.max - camBoundingBox.min;
            sprite.Transform.Translation = Position;
            sprite.scale = scale;
            sprite.Transform.M11 = scale.X;
            sprite.Transform.M22 = scale.Y;
            sprite.viewportScale = sprite.scale / baseImageSize;
            sprite.viewportOffset = Position.Wrapped(baseImageSize) / baseImageSize / sprite.viewportScale;

            sprite.SetColorData(baseImage.Size, baseImage.data);
            sprite.camDistance = float.Epsilon;
            sprite.filtering = stage.backgroundFiltering;

            DrawTransparentSprite(sprite, new BoundingBox2D(Vector2.Zero, resolution), resolution, renderer);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawTransparentSprite(SpriteInfo sprite, BoundingBox2D drawArea, Vector2 resolution, RendererBase renderer)
        {
            drawArea.min = Vector2.Max(Vector2.Zero, drawArea.min);
            drawArea.max = Vector2.Min(resolution, drawArea.max);

            Vector2 framePos = drawArea.min;

            while (framePos.Y < drawArea.max.Y)
            {
                float x = framePos.X;
                float y = framePos.Y;

                if (sprite.camDistance <= zBuffer[(int)x, (int)y])
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                Vector2 screenViewport = framePos / resolution;
                Vector2 camLocal = ScreenToLocal(screenViewport);

                if (!RendererBase.IsWithinMaxExclusive(camLocal.X, camLocal.Y, -1, 1))
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                Vector2 global = LocalToGlobal(camLocal);
                Vector2 spriteLocal = sprite.GlobalToLocal(global);

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

                var colorPos = sprite.LocalToColorPosition(spriteLocal);

                Pixel color = new Pixel();

                switch (sprite.filtering)
                {
                    case TextureFiltering.Point:
                        color = sprite.image.GetPixel((int)colorPos.X, (int)colorPos.Y);
                        break;
                    case TextureFiltering.Bilinear:
                        Vector2 colorSize = sprite.colorDataSize;

                        int left = (int)colorPos.X;
                        int top = (int)colorPos.Y;
                        int right = (int)((left + 1) % colorSize.X);
                        int bottom = (int)((top + 1) % colorSize.Y);

                        float xOffset = colorPos.X - left;
                        float yOffset = colorPos.Y - top;

                        Pixel topJPixel = Pixel.Lerp(sprite.image.GetPixel(left, top), sprite.image.GetPixel(right, top), xOffset);
                        Pixel botJPixel = Pixel.Lerp(sprite.image.GetPixel(left, bottom), sprite.image.GetPixel(right, bottom), xOffset);
                        color = Pixel.Lerp(topJPixel, botJPixel, yOffset);
                        break;
                    default:
                        throw new NotImplementedException(nameof(sprite.filtering));
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

                if (color.a == 255)
                {
                    zBuffer[(int)x, (int)y] = sprite.camDistance;
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
    }
}
