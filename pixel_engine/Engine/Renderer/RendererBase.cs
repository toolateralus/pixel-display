namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Policy;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Bitmap = System.Drawing.Bitmap;
    using Color = System.Drawing.Color;

    public abstract class  RendererBase 
    {
        private protected Color[,] baseImage = new Color[1,1];
        
        private protected byte[] frame = Array.Empty<byte>();
        private protected byte[] latestFrame = Array.Empty<byte>();
        private protected int stride = 0;
        
        public byte[] Frame => latestFrame;
        public int Stride => stride;
        public Vec2 Resolution 
        { 
            get => resolution;
            set => Runtime.Instance.renderHost.newResolution = (Vec2?)value; 
        }

        internal protected Vec2 resolution = Constants.DefaultResolution;

        public bool baseImageDirty = true;

        public abstract void Render(System.Windows.Controls.Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();
        public void RenderCamera(Camera camera, StageRenderInfo renderInfo)
        {
            if (Resolution.y == 0 || Resolution.x == 0) return;

            Node camNode = new("CamNode", camera.parent.Position, Vec2.one);
            Camera cam = camNode.AddComponent(camera.Clone());
            if (cam.zBuffer.GetLength(0) != Resolution.x || cam.zBuffer.GetLength(1) != Resolution.y)
                cam.zBuffer = new float[(int)Resolution.x, (int)Resolution.y];
            Array.Clear(cam.zBuffer);

            DrawBaseImage(cam);
            DrawSprites(renderInfo, cam);
            DrawGraphics(cam);

            if (latestFrame.Length != frame.Length)
                latestFrame = new byte[frame.Length];

            Array.Copy(frame, latestFrame, frame.Length);
        }

        private void DrawGraphics(Camera cam)
        {
            Vec2 framePos = new Vec2();
            foreach(Line line in ShapeDrawer.lines)
            {
                Vec2 startPos = cam.GlobalToScreenViewport(line.startPoint) * Resolution;
                Vec2 endPos = cam.GlobalToScreenViewport(line.endPoint) * Resolution;
                if (startPos == endPos)
                {
                    if (startPos.IsWithinMaxExclusive(Vec2.zero, Resolution))
                        WriteColorToFrame(ref line.color, ref startPos);
                    continue;
                }

                float xDiff = startPos.x - endPos.x;
                float yDiff = startPos.y - endPos.y;

                if (MathF.Abs(xDiff) > MathF.Abs(yDiff))
                {
                    float slope = yDiff / xDiff;
                    float yIntercept = startPos.y - (slope * startPos.x);
                
                    int endX = (int)MathF.Min(MathF.Max(startPos.x, endPos.x), Resolution.x);
                
                    for (int x = (int)MathF.Max(MathF.Min(startPos.x, endPos.x), 0); x < endX; x++)
                    {
                        framePos.x = x;
                        framePos.y = slope * x + yIntercept;
                        if (framePos.y < 0 || framePos.y >= Resolution.y)
                            continue;
                        WriteColorToFrame(ref line.color, ref framePos);
                    }
                }
                else
                {
                    float slope = xDiff / yDiff;
                    float xIntercept = startPos.x - (slope * startPos.y);

                    int endY = (int)MathF.Min(MathF.Max(startPos.y, endPos.y), Resolution.y);

                    for (int y = (int)MathF.Max(MathF.Min(startPos.y, endPos.y), 0); y < endY; y++)
                    {
                        framePos.y = y;
                        framePos.x = slope * y + xIntercept;
                        if (framePos.x < 0 || framePos.x >= Resolution.x)
                            continue;
                        WriteColorToFrame(ref line.color, ref framePos);
                    }
                }
            }
        }

        private void DrawSprites(StageRenderInfo renderInfo, Camera cam)
        {
            Node spriteNode = new("SpriteNode", Vec2.zero, Vec2.one);
            Sprite sprite = spriteNode.AddComponent<Sprite>();
            for (int i = 0; i < renderInfo.Count; ++i)
            {
                sprite.parent.Position = renderInfo.spritePositions[i];
                sprite.ColorData = renderInfo.spriteColorData[i];
                sprite.size = renderInfo.spriteSizeVectors[i];
                sprite.viewportOffset = renderInfo.spriteVPOffsetVectors[i];
                sprite.viewportScale = renderInfo.spriteVPScaleVectors[i];
                sprite.camDistance = renderInfo.spriteCamDistances[i];

                Vec2 firstCorner = cam.GlobalToScreenViewport(sprite.parent.Position) * Resolution;
                //Bounding box on screen which fully captures sprite
                BoundingBox2D drawArea = new(firstCorner, firstCorner);
                List<Vec2> corners = new()
                {
                    cam.GlobalToScreenViewport(sprite.parent.Position + new Vec2(sprite.size.x, 0)) * Resolution,
                    cam.GlobalToScreenViewport(sprite.parent.Position + new Vec2(0, sprite.size.y)) * Resolution,
                    cam.GlobalToScreenViewport(sprite.parent.Position + sprite.size) * Resolution
                };

                foreach (Vec2 corner in corners) drawArea.ExpandTo(corner);

                if (!(drawArea.min).IsWithinMaxExclusive(Vec2.zero, Resolution) &&
                    !(drawArea.max).IsWithinMaxExclusive(Vec2.zero, Resolution)) continue;

                DrawTransparentSprite(cam, sprite, drawArea);
            }
        }

        private void DrawBaseImage(Camera cam)
        {
            Node spriteNode = new("SpriteNode", Vec2.zero, Vec2.one);
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
            sprite.viewportOffset = (cam.Center - cam.bottomRightCornerOffset).Wrapped(baseImageSize) / baseImageSize / sprite.viewportScale;
            sprite.ColorData = baseImage;
            sprite.camDistance = float.Epsilon;
            DrawTransparentSprite(cam, sprite, new BoundingBox2D(Vec2.zero, Resolution));
        }

        private void DrawTransparentSprite(Camera cam, Sprite sprite, BoundingBox2D drawArea)
        {
            Vec2 colorPos = new();
            Vec2 camViewport = new();
            //Vec2 colorPos = new();
            for (Vec2 framePos = drawArea.min;
                framePos.y < drawArea.max.y;
                framePos.Increment2D(drawArea.max.x, drawArea.min.x))
            {
                if (!framePos.IsWithinMaxExclusive(Vec2.zero, Resolution)) continue;
                if (sprite.camDistance <= cam.zBuffer[(int)framePos.x, (int)framePos.y]) continue;

                //this is actually cam viewport here, just reusing Vec2 to avoid new() calls
                camViewport = cam.ScreenToCamViewport(framePos / Resolution);
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                //this is also sprite viewport here
                colorPos = cam.ViewportToSpriteViewport(sprite, camViewport);
                if (!colorPos.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

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

