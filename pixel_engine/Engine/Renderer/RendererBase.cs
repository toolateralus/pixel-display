﻿namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Policy;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Bitmap = System.Drawing.Bitmap;
    using Color = System.Drawing.Color;

    public abstract class  RendererBase 
    {
        Vec2 zero = Vec2.zero;
        Vec2 one = Vec2.one;

        private protected Color[,] baseImage = new Color[1,1];
        
        private protected byte[] frame = Array.Empty<byte>();
        private protected byte[] latestFrame = Array.Empty<byte>();
        private protected int stride = 0;
        
        public byte[] Frame => latestFrame;
        public int Stride => stride;
        public Vec2 Resolution 
        { 
            get => _resolution;
            set => Runtime.Current.renderHost.newResolution = (Vec2?)value; 
        }

        internal protected Vec2 _resolution = Constants.DefaultResolution;

        public bool baseImageDirty = true;

        public abstract void Render(System.Windows.Controls.Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();
        public void RenderCamera(Camera camera, StageRenderInfo renderInfo, Vec2 resolution)
        {
            if (resolution.y == 0 || resolution.x == 0) return;

            Node camNode = new("CamNode", camera.parent.Position, one);
            Camera cam = camNode.AddComponent(camera.Clone());
            if (cam.zBuffer.GetLength(0) != resolution.x || cam.zBuffer.GetLength(1) != resolution.y)
                cam.zBuffer = new float[(int)resolution.x, (int)resolution.y];
            Array.Clear(cam.zBuffer);

            DrawBaseImage(cam, resolution);
            DrawSprites(renderInfo, cam, resolution);
            DrawGraphics(cam, resolution);

            if (latestFrame.Length != frame.Length)
                latestFrame = new byte[frame.Length];

            Array.Copy(frame, latestFrame, frame.Length);
        }

        private void DrawGraphics(Camera cam, Vec2 resolution)
        {
            Vec2 framePos = new Vec2();
            foreach (Circle circle in ShapeDrawer.circles)
            {
                Vec2 centerPos = cam.GlobalToScreenViewport(circle.center) * resolution;
                float pixelRadius = (cam.GlobalToScreenViewport(new Vec2(circle.radius,0)) * resolution).x;
                for (Vec2 pixel = one * -pixelRadius; pixel.y < pixelRadius; pixel.Increment2D(pixelRadius, -pixelRadius))
                {
                    if ((int)pixel.SqrMagnitude() == (int)pixelRadius * (int)pixelRadius)
                    {
                        framePos = centerPos + pixel;
                        if (!framePos.IsWithinMaxExclusive(zero, resolution))
                            continue;
                        WriteColorToFrame(ref circle.color, ref framePos);
                    }
                }
            }
            foreach(Line line in ShapeDrawer.lines)
            {
                Vec2 startPos = cam.GlobalToScreenViewport(line.startPoint) * resolution;
                Vec2 endPos = cam.GlobalToScreenViewport(line.endPoint) * resolution;
                if (startPos == endPos)
                {
                    if (startPos.IsWithinMaxExclusive(zero, resolution))
                        WriteColorToFrame(ref line.color, ref startPos);
                    continue;
                }

                float xDiff = startPos.x - endPos.x;
                float yDiff = startPos.y - endPos.y;

                if (MathF.Abs(xDiff) > MathF.Abs(yDiff))
                {
                    float slope = yDiff / xDiff;
                    float yIntercept = startPos.y - (slope * startPos.x);
                
                    int endX = (int)MathF.Min(MathF.Max(startPos.x, endPos.x), resolution.x);
                
                    for (int x = (int)MathF.Max(MathF.Min(startPos.x, endPos.x), 0); x < endX; x++)
                    {
                        framePos.x = x;
                        framePos.y = slope * x + yIntercept;
                        if (framePos.y < 0 || framePos.y >= resolution.y)
                            continue;
                        WriteColorToFrame(ref line.color, ref framePos);
                    }
                }
                else
                {
                    float slope = xDiff / yDiff;
                    float xIntercept = startPos.x - (slope * startPos.y);

                    int endY = (int)MathF.Min(MathF.Max(startPos.y, endPos.y), resolution.y);

                    for (int y = (int)MathF.Max(MathF.Min(startPos.y, endPos.y), 0); y < endY; y++)
                    {
                        framePos.y = y;
                        framePos.x = slope * y + xIntercept;
                        if (framePos.x < 0 || framePos.x >= resolution.x)
                            continue;
                        WriteColorToFrame(ref line.color, ref framePos);
                    }
                }
            }
        }

        private void DrawSprites(StageRenderInfo renderInfo, Camera cam, Vec2 resolution)
        {
            Node spriteNode = new("SpriteNode", zero, one);
            Sprite sprite = spriteNode.AddComponent<Sprite>();
            for (int i = 0; i < renderInfo.Count; ++i)
            {
                sprite.parent.Position = renderInfo.spritePositions[i];
                sprite.ColorData = renderInfo.spriteColorData[i];
                sprite.size = renderInfo.spriteSizeVectors[i];
                sprite.viewportOffset = renderInfo.spriteVPOffsetVectors[i];
                sprite.viewportScale = renderInfo.spriteVPScaleVectors[i];
                sprite.camDistance = renderInfo.spriteCamDistances[i];

                Vec2 firstCorner = cam.GlobalToScreenViewport(sprite.parent.Position) * resolution;
                //Bounding box on screen which fully captures sprite
                BoundingBox2D drawArea = new(firstCorner, firstCorner);
                List<Vec2> corners = new()
                {
                    cam.GlobalToScreenViewport(sprite.parent.Position + new Vec2(sprite.size.x, 0)) * resolution,
                    cam.GlobalToScreenViewport(sprite.parent.Position + new Vec2(0, sprite.size.y)) * resolution,
                    cam.GlobalToScreenViewport(sprite.parent.Position + sprite.size) * resolution
                };

                foreach (Vec2 corner in corners) drawArea.ExpandTo(corner);

                if (!(drawArea.min).IsWithinMaxExclusive(zero, resolution) &&
                    !(drawArea.max).IsWithinMaxExclusive(zero, resolution)) continue;

                DrawTransparentSprite(cam, sprite, drawArea, resolution);
            }
        }

        private void DrawBaseImage(Camera cam, Vec2 resolution)
        {
            Node spriteNode = new("SpriteNode", zero, one);
            Sprite sprite = spriteNode.AddComponent<Sprite>();
            Vec2 baseImageSize = new(Constants.ScreenH, Constants.ScreenW);
            Vec2 topLeft = cam.Center - cam.bottomRightCornerOffset.Rotated(cam.angle);
            BoundingBox2D camBoundingBox = new(topLeft, topLeft);
            List<Vec2> camCorners = new()
            {
                cam.Center + cam.bottomRightCornerOffset.WithScale(-1, 1).Rotated(cam.angle),
                cam.Center + cam.bottomRightCornerOffset.WithScale(1, -1).Rotated(cam.angle),
                cam.Center + cam.bottomRightCornerOffset.Rotated(cam.angle)
            };

            foreach (Vec2 corner in camCorners) camBoundingBox.ExpandTo(corner);

            sprite.parent.Position = camBoundingBox.min;
            sprite.size = camBoundingBox.max - camBoundingBox.min;
            sprite.viewportScale = sprite.size / baseImageSize;
            sprite.viewportOffset = (cam.Center - cam.bottomRightCornerOffset).Wrapped(baseImageSize) / baseImageSize / sprite.viewportScale.GetDivideSafe();
            sprite.ColorData = baseImage;
            sprite.camDistance = float.Epsilon;
            DrawTransparentSprite(cam, sprite, new BoundingBox2D(zero, resolution), resolution);
        }

        private void DrawTransparentSprite(Camera cam, Sprite sprite, BoundingBox2D drawArea, Vec2 resolution)
        {
            Vec2 colorPos = new();
            Vec2 camViewport = new();
            //Vec2 colorPos = new();
            for (Vec2 framePos = drawArea.min;
                framePos.y < drawArea.max.y;
                framePos.Increment2D(drawArea.max.x, drawArea.min.x))
            {
                if (!framePos.IsWithinMaxExclusive(zero, resolution)) continue;
                if (sprite.camDistance <= cam.zBuffer[(int)framePos.x, (int)framePos.y]) continue;

                //this is actually cam viewport here, just reusing Vec2 to avoid new() calls
                camViewport = cam.ScreenToCamViewport(framePos / resolution);
                if (!camViewport.IsWithinMaxExclusive(zero, one)) continue;

                //this is also sprite viewport here
                colorPos = cam.ViewportToSpriteViewport(sprite, camViewport);
                if (!colorPos.IsWithinMaxExclusive(zero, one)) continue;

                colorPos = sprite.ViewportToColorPos(colorPos);
                if (sprite.ColorData[(int)colorPos.x, (int)colorPos.y].A == 0) continue;

                if (sprite.ColorData[(int)colorPos.x, (int)colorPos.y].A == 255)
                    cam.zBuffer[(int)framePos.x, (int)framePos.y] = sprite.camDistance;

                WriteColorToFrame(ref sprite.ColorData[(int)colorPos.x, (int)colorPos.y], ref framePos);
            }
        }

        private void WriteColorToFrame(ref Color color, ref Vec2 framePos)
        {
            int index = (int)framePos.y * stride + ((int)framePos.x * 3);

            float colorB = (float)color.B / 255 * color.A;
            float colorG = (float)color.G / 255 * color.A;
            float colorR = (float)color.R / 255 * color.A;

            float frameB = (float)frame[index + 0] / 255 * (255 - color.A);
            float frameG = (float)frame[index + 1] / 255 * (255 - color.A);
            float frameR = (float)frame[index + 2] / 255 * (255 - color.A);

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

