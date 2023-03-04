﻿using Newtonsoft.Json;
using pixel_renderer.ShapeDrawing;
using System;
using System.Linq;
using System.Numerics;
using System.Printing.IndexedProperties;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace pixel_renderer
{
    public class Camera : UIComponent
    {
        [Field] [JsonProperty] public Vector2 viewportPosition = Vector2.Zero;
        [Field] [JsonProperty] public Vector2 viewportSize = Vector2.One;
        public float[,] zBuffer = new float[0, 0];
        public Vector2 LocalToScreenViewport(Vector2 local) => local * viewportSize + viewportPosition;
        public Vector2 ScreenViewportToLocal(Vector2 screenViewport)
        {
            viewportSize.MakeDivideSafe();
            return (screenViewport - viewportPosition) / viewportSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RenderCamera(Camera cam, StageRenderInfo renderInfo, Vector2 resolution, ref byte[] frame, ref byte[] latestFrame, JImage baseImage, RendererBase renderer)
        {
            if (resolution.Y == 0 || resolution.X == 0) return;

            if (cam.zBuffer.GetLength(0) != resolution.X || cam.zBuffer.GetLength(1) != resolution.Y)
                cam.zBuffer = new float[(int)resolution.X, (int)resolution.Y];
            Array.Clear(cam.zBuffer);

            DrawBaseImage(cam, resolution, baseImage, renderer);
            DrawSprites(renderInfo, cam, resolution, renderer);
            DrawGraphics(cam, resolution, renderer);

            if (latestFrame.Length != frame.Length)
                latestFrame = new byte[frame.Length];

            Array.Copy(frame, latestFrame, frame.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawGraphics(Camera cam, Vector2 resolution, RendererBase renderer)
        {
            Vector2 framePos = new Vector2();
            foreach (pixel_renderer.ShapeDrawing.Circle circle in ShapeDrawer.Circles)
            {
                float sqrtOfHalf = MathF.Sqrt(0.5f);
                Vector2 radius = circle.center + new Vector2(circle.radius, circle.radius);
                Vector2 centerPos = cam.GlobalToScreenViewport(circle.center) * resolution;
                Vector2 pixelRadius = cam.GlobalToScreenViewport(radius) * resolution - centerPos;
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
                Vector2 startPos = cam.GlobalToScreenViewport(line.startPoint) * resolution;
                Vector2 endPos = cam.GlobalToScreenViewport(line.endPoint) * resolution;
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
        private void DrawSprites(StageRenderInfo renderInfo, Camera cam, Vector2 resolution, RendererBase renderer)
        {
            SpriteInfo sprite;
            BoundingBox2D drawArea = new();
            for (int i = 0; i < renderInfo.spriteInfos.Count; ++i)
            {
                sprite = renderInfo.spriteInfos[i];
                drawArea = new(sprite.GetCorners());
                drawArea.min = cam.GlobalToLocal(drawArea.min);
                drawArea.max = cam.GlobalToLocal(drawArea.max);
                if (drawArea.min.X >= 1 || drawArea.max.X < 0 ||
                    drawArea.min.Y >= 1 || drawArea.max.Y < 0)
                    continue;
                drawArea.min = cam.LocalToScreenViewport(drawArea.min) * resolution;
                drawArea.max = cam.LocalToScreenViewport(drawArea.max) * resolution;

                DrawTransparentSprite(cam, sprite, drawArea, resolution, renderer);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawBaseImage(Camera cam, Vector2 resolution, JImage baseImage, RendererBase renderer)
        {
            SpriteInfo sprite = new();

            var stage = Runtime.Current.GetStage();
            Vector2 baseImageSize;

            if (stage != null)
                baseImageSize = stage.backgroundSize;
            else baseImageSize = new(Constants.ScreenH, Constants.ScreenW);

            BoundingBox2D camBoundingBox = new(cam.GetCorners());

            var scale = camBoundingBox.max - camBoundingBox.min;
            sprite.Transform.Translation = cam.Center;
            sprite.scale = scale;
            sprite.Transform.M11 = scale.X;
            sprite.Transform.M22 = scale.Y;
            sprite.viewportScale = sprite.scale / baseImageSize;

            sprite.viewportScale.MakeDivideSafe();
            sprite.viewportOffset = cam.Center.Wrapped(baseImageSize) / baseImageSize / sprite.viewportScale;

            sprite.SetColorData(baseImage.Size, baseImage.data);
            sprite.camDistance = float.Epsilon;
            sprite.filtering = stage.backgroundFiltering;

            DrawTransparentSprite(cam, sprite, new BoundingBox2D(Vector2.Zero, resolution), resolution, renderer);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawTransparentSprite(Camera cam, SpriteInfo sprite, BoundingBox2D drawArea, Vector2 resolution, RendererBase renderer)
        {
            drawArea.min = Vector2.Max(Vector2.Zero, drawArea.min);
            drawArea.max = Vector2.Min(resolution, drawArea.max);

            Vector2 framePos = drawArea.min;

            while (framePos.Y < drawArea.max.Y)
            {
                float x = framePos.X;
                float y = framePos.Y;

                if (sprite.camDistance <= cam.zBuffer[(int)x, (int)y])
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
                Vector2 camLocal = cam.ScreenViewportToLocal(screenViewport);

                if (!RendererBase.IsWithinMaxExclusive(camLocal.X, camLocal.Y, 0, 1))
                {
                    framePos.X++;
                    if (framePos.X >= drawArea.max.X)
                    {
                        framePos.X = drawArea.min.X;
                        framePos.Y++;
                    }
                    continue;
                }

                Vector2 global = cam.LocalToGlobal(camLocal);
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
                    cam.zBuffer[(int)x, (int)y] = sprite.camDistance;
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


        public Vector2 GlobalToScreenViewport(Vector2 global) => LocalToScreenViewport(GlobalToLocal(global));
        public Vector2 ScreenViewportToGlobal(Vector2 screenViewport) => LocalToGlobal(ScreenViewportToLocal(screenViewport));
        public Vector2 LocalToSpriteViewport(Sprite sprite, Vector2 local) =>
            sprite.GlobalToViewport(LocalToGlobal(local));
        public Vector2 LocalToSpriteLocal(SpriteInfo sprite, Vector2 local) =>
            sprite.GlobalToLocal(LocalToGlobal(local));
        public static Camera? First => Runtime.Current.GetStage()?.GetAllComponents<Camera>().First();
    }
    public enum DrawingType { Wrapped, Clamped, None }
}
