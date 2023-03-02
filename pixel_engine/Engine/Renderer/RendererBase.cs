﻿namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Security.Policy;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Bitmap = System.Drawing.Bitmap;

    public abstract class  RendererBase 
    {
        Vector2 zero = Vector2.Zero;
        Vector2 one = Vector2.One;
        private protected JImage baseImage;
        private protected byte[] frame = Array.Empty<byte>();
        private protected byte[] latestFrame = Array.Empty<byte>();
        private protected int stride = 0;
        public byte[] Frame => latestFrame;
        public int Stride => stride;
        public Vector2 Resolution 
        { 
            get => _resolution;
            set => Runtime.Current.renderHost.newResolution = value; 
        }
        internal protected Vector2 _resolution = Constants.DefaultResolution;
        public bool baseImageDirty = true;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public abstract void Render(System.Windows.Controls.Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RenderCamera(Camera camera, StageRenderInfo renderInfo, Vector2 resolution)
        {
            if (resolution.Y == 0 || resolution.X == 0) return;

            Node camNode = new("CamNode", camera.parent.Position, one);
            Camera cam = camNode.AddComponent(camera.Clone());
            if (cam.zBuffer.GetLength(0) != resolution.X || cam.zBuffer.GetLength(1) != resolution.Y)
                cam.zBuffer = new float[(int)resolution.X , (int)resolution.Y];
            Array.Clear(cam.zBuffer);

            DrawBaseImage(cam, resolution);
            DrawSprites(renderInfo, cam, resolution);
            DrawGraphics(cam, resolution);

            if (latestFrame.Length != frame.Length)
                latestFrame = new byte[frame.Length];

            Array.Copy(frame, latestFrame, frame.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawGraphics(Camera cam, Vector2 resolution)
        {
            Vector2 framePos = new Vector2();
            foreach (Circle circle in ShapeDrawer.Circles)
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
                    if (framePos.IsWithinMaxExclusive(zero, resolution))
                        WriteColorToFrame(ref circle.color, ref framePos);
                    framePos.Y = centerPos.Y - y;
                    if (framePos.IsWithinMaxExclusive(zero, resolution))
                        WriteColorToFrame(ref circle.color, ref framePos);
                }
                quarterArcAsInt = (int)quaterArc.Y;
                for (int y = -quarterArcAsInt; y <= quarterArcAsInt; y++)
                {
                    float x = MathF.Cos(MathF.Asin(y / pixelRadius.Y)) * pixelRadius.X;
                    framePos.Y = centerPos.Y + y;
                    framePos.X = centerPos.X + x;
                    if (framePos.IsWithinMaxExclusive(zero, resolution))
                        WriteColorToFrame(ref circle.color, ref framePos);
                    framePos.X = centerPos.X - x;
                    if (framePos.IsWithinMaxExclusive(zero, resolution))
                        WriteColorToFrame(ref circle.color, ref framePos);
                }
            }
            foreach(Line line in ShapeDrawer.Lines)
            {
                Vector2 startPos = cam.GlobalToScreenViewport(line.startPoint) * resolution;
                Vector2 endPos = cam.GlobalToScreenViewport(line.endPoint) * resolution;
                if (startPos == endPos)
                {
                    if (startPos.IsWithinMaxExclusive(zero, resolution))
                        WriteColorToFrame(ref line.color, ref startPos);
                    continue;
                }

                float xDiff = startPos.X - endPos.X;
                float yDiff = startPos.Y - endPos.Y;

                if (MathF.Abs(xDiff) > MathF.Abs(yDiff))
                {
                    float slope = yDiff / xDiff;
                    float yIntercept = startPos.Y - (slope * startPos.X);
                
                    int endX = (int)MathF.Min(MathF.Max(startPos.X ,endPos.X), resolution.X);
                
                    for (int x = (int)MathF.Max(MathF.Min(startPos.X ,endPos.X), 0); x < endX; x++)
                    {
                        framePos.X = x;
                        framePos.Y = slope * x + yIntercept;
                        if (framePos.Y < 0 || framePos.Y >= resolution.Y)
                            continue;
                        WriteColorToFrame(ref line.color, ref framePos);
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
                        WriteColorToFrame(ref line.color, ref framePos);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawSprites(StageRenderInfo renderInfo, Camera cam, Vector2 resolution)
        {
            Node spriteNode = new("SpriteNode", zero, one);
            Sprite sprite = spriteNode.AddComponent<Sprite>();
            BoundingBox2D drawArea = new();
            for (int i = 0; i < renderInfo.Count; ++i)
            {
                renderInfo.SetSprite(sprite, i);

                Vector2 spritePos = sprite.parent.Position;
                Vector2 firstCorner = cam.GlobalToScreenViewport(spritePos) * resolution;
                //Bounding box on screen which fully captures sprite
                drawArea.min = firstCorner;
                drawArea.max = firstCorner;
                List<Vector2> corners = new()
                {
                    cam.GlobalToScreenViewport(spritePos + new Vector2(sprite.size.X ,0)) * resolution,
                    cam.GlobalToScreenViewport(spritePos + new Vector2(0, sprite.size.Y)) * resolution,
                    cam.GlobalToScreenViewport(spritePos + sprite.size) * resolution
                };

                foreach (Vector2 corner in corners)
                    drawArea.ExpandTo(corner);

                if (!drawArea.min.IsWithinMaxExclusive(zero, resolution) &&
                    !drawArea.max.IsWithinMaxExclusive(zero, resolution)) continue;

                DrawTransparentSprite(cam, sprite, drawArea, resolution);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawBaseImage(Camera cam, Vector2 resolution)
        {
            Node spriteNode = new("SpriteNode", zero, one);
            Sprite sprite = spriteNode.AddComponent<Sprite>();

            var stage = Runtime.Current.GetStage();
            Vector2 baseImageSize;
            
            if (stage != null)
                baseImageSize = stage.backgroundSize;
            else baseImageSize = new(Constants.ScreenH, Constants.ScreenW);


            Vector2 topLeft = cam.Center - cam.bottomRightCornerOffset.Rotated(cam.angle);
            BoundingBox2D camBoundingBox = new(topLeft, topLeft);


            List<Vector2> camCorners = new()
            {
                cam.Center + cam.bottomRightCornerOffset.WithScale(-1, 1).Rotated(cam.angle),
                cam.Center + cam.bottomRightCornerOffset.WithScale(1, -1).Rotated(cam.angle),
                cam.Center + cam.bottomRightCornerOffset.Rotated(cam.angle)
            };

            foreach (Vector2 corner in camCorners) camBoundingBox.ExpandTo(corner);

            sprite.parent.Position = camBoundingBox.min;
            sprite.size = camBoundingBox.max - camBoundingBox.min;
            sprite.viewportScale = sprite.size / baseImageSize;

            sprite.viewportScale.GetDivideSafeRef();
            sprite.viewportOffset = (cam.Center - cam.bottomRightCornerOffset).Wrapped(baseImageSize) / baseImageSize / sprite.viewportScale;
            sprite.SetColorData(baseImage.Size, baseImage.data);
            sprite.camDistance = float.Epsilon;
            sprite.textureFiltering = stage.backgroundFiltering;
            DrawTransparentSprite(cam, sprite, new BoundingBox2D(zero, resolution), resolution);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DrawTransparentSprite(Camera cam, Sprite sprite, BoundingBox2D drawArea, Vector2 resolution)
        {
            for (Vector2 framePos = drawArea.min;
                framePos.Y < drawArea.max.Y;
                framePos.Increment2D(drawArea.max.X ,drawArea.min.X))
            {
                if (!framePos.IsWithinMaxExclusive(zero, resolution))
                    continue;
                if (sprite.camDistance <= cam.zBuffer[(int)framePos.X ,(int)framePos.Y])
                    continue;
                Vector2 camViewport = cam.ScreenToCamViewport(framePos / resolution);
                if (!camViewport.IsWithinMaxExclusive(zero, one))
                    continue;
                Vector2 spriteViewportPos = cam.ViewportToSpriteViewport(sprite, camViewport);
                if (!spriteViewportPos.IsWithinMaxExclusive(zero, one))
                    continue;
                Vector2 colorPos = sprite.ViewportToColorPos(spriteViewportPos);

                Pixel color = new();

                switch (sprite.textureFiltering)
                {
                    case TextureFiltering.Point:
                        color = sprite.texture.GetPixel((int)colorPos.X ,(int)colorPos.Y);
                        break;
                    case TextureFiltering.Bilinear:
                        Vector2 colorSize = sprite.ColorDataSize;
                        int left = (int)colorPos.X;
                        int top = (int)colorPos.Y;
                        int right = (int)((left + 1) % colorSize.X);
                        int bottom = (int)((top + 1) % colorSize.Y);
                        float xOffset = colorPos.X - left;
                        float yOffset = colorPos.Y - top;
                        Pixel topJPixel = Pixel.Lerp(sprite.texture.GetPixel(left, top), sprite.texture.GetPixel(right, top), xOffset);
                        Pixel botJPixel = Pixel.Lerp(sprite.texture.GetPixel(left, bottom), sprite.texture.GetPixel(right, bottom), xOffset);
                        color = Pixel.Lerp(topJPixel, botJPixel, yOffset);
                        break;
                    default:
                        throw new NotImplementedException(nameof(sprite.textureFiltering));
                }
                if (color.a == 0)
                    continue;
                if (color.a == 255)
                    cam.zBuffer[(int)framePos.X, (int)framePos.Y] = sprite.camDistance;
                WriteColorToFrame(ref color, ref framePos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void WriteColorToFrame(ref Pixel color, ref Vector2 framePos)
        {
            int index = (int)framePos.Y * stride + ((int)framePos.X * 3);

            float colorB = (float)color.b / 255 * color.a;
            float colorG = (float)color.g / 255 * color.a;
            float colorR = (float)color.r / 255 * color.a;

            float frameB = (float)frame[index + 0] / 255 * (255 - color.a);
            float frameG = (float)frame[index + 1] / 255 * (255 - color.a);
            float frameR = (float)frame[index + 2] / 255 * (255 - color.a);

            frame[index + 0] = (byte)(colorB + frameB);
            frame[index + 1] = (byte)(colorG + frameG);
            frame[index + 2] = (byte)(colorR + frameR);
        }
        internal void MarkDirty()
        {
            baseImageDirty = true;
        }
    }
}

