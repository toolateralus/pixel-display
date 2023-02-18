namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;
    using Color = System.Drawing.Color;

    public abstract class  RendererBase 
    {
        private protected Color[,] baseImage = new Color[1,1];
        private protected byte[] frame = Array.Empty<byte>();
        private protected int stride = 0;

        public Vec2 Resolution = Constants.DefaultResolution;

        public bool baseImageDirty = true;

        public abstract void Render(System.Windows.Controls.Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();
        private static Vec2 ViewportToPosWithDrawingType(Camera cam, Vec2 size, Vec2 ViewportPos)
        {
            Vec2 maxIndex = size - Vec2.one;
            return cam.DrawMode switch
            {
                DrawingType.Wrapped => ViewportPos.Wrapped(Vec2.one) * size,
                DrawingType.Clamped => (ViewportPos.Clamped(Vec2.zero, Vec2.one) * maxIndex).Clamped(Vec2.zero, maxIndex),
                _ => new(0, 0),
            };
        }
        private void DrawBackground(Camera cam)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Vec2 backgroundSize = new(baseImage.GetLength(0), baseImage.GetLength(1));

            for (Vec2 framePos = new(0,0); framePos.y < Resolution.y; framePos.Increment2D(Resolution.x))
            {
                Vec2 camViewport = cam.ScreenToCamViewport(framePos / Resolution);
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 global = cam.ViewportToGlobal(camViewport);
                Vec2 bgViewportPos = global / backgroundSize.GetDivideSafe();
                Vec2 bgPos = ViewportToPosWithDrawingType(cam, backgroundSize, bgViewportPos);

                WriteColorToFrame(ref baseImage[(int)bgPos.x, (int)bgPos.y], ref framePos);
            }
        }
        public void RenderSprites(Camera camera, StageRenderInfo renderInfo)
        {
            if (Resolution.y == 0 || Resolution.x == 0) return;
            Node camNode = new("CamNode", camera.parent.Position, Vec2.one);
            Camera cam = camNode.AddComponent(camera.Clone());
            
            if (cam.zBuffer.GetLength(0) != Resolution.x || cam.zBuffer.GetLength(1) != Resolution.y)
                cam.zBuffer = new float[(int)Resolution.x, (int)Resolution.y];

            Array.Clear(cam.zBuffer);

            DrawBackground(cam);

            Node spriteNode = new("SpriteNode", Vec2.zero, Vec2.one);
            Sprite sprite = spriteNode.AddComponent<Sprite>();

            //sprite.parent.Position = Vec2.zero;
            //sprite.ColorData = baseImage;
            //sprite.size = new Vec2(Constants.ScreenH, Constants.ScreenW);
            //sprite.camDistance = float.Epsilon;

            //DrawTransparentSprite(cam, sprite, new BoundingBox2D(Vec2.zero, Resolution));

            for (int i = 0; i < renderInfo.Count; ++i)
            {
                sprite.parent.Position = renderInfo.spritePositions[i];
                sprite.ColorData = renderInfo.spriteColorData[i];
                sprite.size = renderInfo.spriteSizeVectors[i];
                sprite.viewportOffset = renderInfo.spriteVPOffsetVectors[i];
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

        private void DrawTransparentSprite(Camera cam, Sprite sprite, BoundingBox2D drawArea)
        {
            for (Vec2 framePos = drawArea.min;
                framePos.y < drawArea.max.y;
                framePos.Increment2D(drawArea.max.x, drawArea.min.x))
            {
                if (!framePos.IsWithinMaxExclusive(Vec2.zero, Resolution)) continue;
                if (sprite.camDistance <= cam.zBuffer[(int)framePos.x, (int)framePos.y]) continue;

                //this is actually cam viewport here, just reusing Vec2 to avoid new() calls
                Vec2 colorPos = cam.ScreenToCamViewport(framePos / Resolution);
                if (!colorPos.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                //this is also sprite viewport here
                colorPos = cam.ViewportToSpriteViewport(sprite, colorPos);
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

