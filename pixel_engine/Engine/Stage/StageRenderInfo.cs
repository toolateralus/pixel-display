using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows.Controls;

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
            for (int i = 0; i < objectCount; ++i)
                infoList[i].Set(objects.ElementAt(i));
        }
    }
    public abstract class ViewpoortInfoObject
    {
        public Matrix3x2 transform;
        public Matrix3x2 transformInverted;
        public Matrix3x2 projectionMat;
        public Matrix3x2 projectionMatInverted;
        public readonly Matrix3x2 screenMat = Matrix3x2.CreateTranslation(1, 1) * Matrix3x2.CreateScale(0.5f, 0.5f);
        public readonly Matrix3x2 screenMatInverted = (Matrix3x2.CreateTranslation(1, 1) * Matrix3x2.CreateScale(0.5f, 0.5f)).Inverted();
        public readonly Vector2 oneVect = Vector2.One;
        public readonly Vector2 zeroVect;
        public Vector2 ViewportOffset
        {
            get => projectionMat.Translation;
            set => projectionMat.Translation = value;
        }
        public Vector2 ViewportScale
        {
            set => projectionMat.SetScale(value);
            get => new(projectionMat.M11, projectionMat.M22);
        }
        public Vector2 Position
        {
            get => transform.Translation;
            set => transform.Translation = value;
        }
        public Vector2[] GetCorners()
        {
            var viewport = Polygon.UnitSquare();
            viewport.Transform(transform);
            return viewport.vertices;
        }
        public Vector2 LocalToScreen(Vector2 local)
        {
            local.Transform(projectionMat);
            local.Transform(screenMat);
            return local;
        }
        public Vector2 ScreenToLocal(Vector2 screenViewport)
        {
            screenViewport.Transform(screenMatInverted);
            screenViewport.Transform(projectionMatInverted);
            return screenViewport;
        }
        public Vector2 LocalToGlobal(Vector2 local)
        {
            local.Transform(transform);
            return local;
        }

        internal Vector2 GlobalToLocal(Vector2 global)
        {
            global.Transform(transformInverted);
            return global;
        }

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
            ViewportOffset = sprite.viewportOffset;
            ViewportScale = sprite.viewportScale.GetDivideSafe();
            camDistance = sprite.camDistance;
            projectionMatInverted = projectionMat.Inverted();
            
            image = sprite.texture.GetImage();
            colorDataSize = image.Size;
            filtering = sprite.textureFiltering;

            transform = sprite.Transform;
            transformInverted = transform.Inverted();
            scale = sprite.Scale;
        }
        public Vector2 LocalToColorPosition(Vector2 local)
        {
            local.Transform(projectionMat);
            return ViewportToColorPosition(local);
        }

        public Vector2 ViewportToColorPosition(Vector2 viewport)
        {
            viewport.X += 0.5f;
            viewport.Y += 0.5f;
            return viewport.Wrapped(oneVect) * colorDataSize;
        }
        public void SetColorData(Vector2 size, byte[] data)
        {
            image = new(size, data);
            colorDataSize = new(size.X, size.Y);
        }
        public void GetFilteredPixel(in Vector2 position, out Pixel output)
        {
            switch (filtering)
            {
                case TextureFiltering.Point:
                    image.GetPixel((int)position.X, (int)position.Y, out output);
                    break;
                case TextureFiltering.Bilinear:
                    int left = (int)position.X;
                    int top = (int)position.Y;
                    int right = (int)((left + 1) % colorDataSize.X);
                    int bottom = (int)((top + 1) % colorDataSize.Y);

                    float xOffset = position.X - left;
                    float yOffset = position.Y - top;

                    Pixel topJPixel = Pixel.Lerp(image.GetPixel(left, top), image.GetPixel(right, top), xOffset);
                    Pixel botJPixel = Pixel.Lerp(image.GetPixel(left, bottom), image.GetPixel(right, bottom), xOffset);
                    output = Pixel.Lerp(topJPixel, botJPixel, yOffset);
                    break;
                default:
                    throw new NotImplementedException(nameof(filtering));
            }
        }
    }
    public class CameraInfo : ViewpoortInfoObject
    {
        public float[,] zBuffer = new float[0, 0];

        public override void Set(object? refObject)
        {
            if (refObject is not Camera cam)
                throw new ArgumentException($"Must pass in object of type {nameof(Camera)}");
            ViewportOffset = cam.viewportPosition;
            ViewportScale = cam.viewportSize.GetDivideSafe();
            projectionMatInverted = projectionMat.Inverted();
            transform = cam.Transform;
            transformInverted = transform.Inverted();
            cam.camInfo = this;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Draw(RendererBase renderer)
        {
            Vector2 resolution = renderer.Resolution;
            if (resolution.Y == 0 || resolution.X == 0) return;

            if (zBuffer.GetLength(0) != resolution.X || zBuffer.GetLength(1) != resolution.Y)
                zBuffer = new float[(int)resolution.X, (int)resolution.Y];

            Array.Clear(zBuffer);

            DrawBaseImage(resolution, renderer.baseImage, renderer);
            if (Runtime.Current.GetStage() is Stage stage)
                DrawSprites(stage.StageRenderInfo, resolution, renderer);
            ShapeDrawer.DrawGraphics(renderer, transform.Inverted(), projectionMat);
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

            if(Runtime.Current.GetStage() is not Stage stage)
                return;
            sprite.transform = transform;
            sprite.transformInverted = transform.Inverted();

            sprite.projectionMat = transform * stage.bgTransform.Inverted();
            sprite.projectionMatInverted = sprite.projectionMat.Inverted();

            sprite.SetColorData(baseImage.Size, baseImage.data);
            sprite.camDistance = float.Epsilon;
            sprite.filtering = stage.backgroundFiltering;

            DrawTransparentSprite(sprite, new BoundingBox2D(zeroVect, resolution), resolution, renderer);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawTransparentSprite(SpriteInfo sprite, BoundingBox2D drawArea, Vector2 resolution, RendererBase renderer)
        {
            drawArea.min = Vector2.Max(zeroVect, drawArea.min);
            drawArea.max = Vector2.Min(resolution, drawArea.max);

            Vector2 framePos = drawArea.min;
            Vector2 output;

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

                output = framePos / resolution; //frame to projection uncentered
                output.Transform(screenMatInverted); //center
                output.Transform(projectionMatInverted); //projection to cam view

                if (!RendererBase.IsWithinMaxExclusive(output.X, output.Y, -1, 1))
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                output.Transform(transform); //cam view to world
                output.Transform(sprite.transformInverted); //world to sprite view

                if (!RendererBase.IsWithinMaxExclusive(output.X, output.Y, -1, 1))
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                //output = sprite.LocalToColorPosition(output);
                output.Transform(sprite.projectionMat); // sprite view to projection

                output.X += 0.5f; // uncenter projection for texture coord
                output.Y += 0.5f;
                output.X -= MathF.Floor(output.X); // wrap X 0-1
                if (output.X < 0) output.X += 1;
                output.Y -= MathF.Floor(output.Y); // wrap Y 0-1
                if (output.Y < 0) output.Y += 1;
                output *= sprite.colorDataSize; // scale texture coord to img size

                sprite.GetFilteredPixel(output, out var color);

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
