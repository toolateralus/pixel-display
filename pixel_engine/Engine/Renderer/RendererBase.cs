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
        private protected Bitmap fallback;
        private protected Color[,] baseImage = new Color[1,1];
        private protected byte[] frame = Array.Empty<byte>();
        private protected int stride = 0;

        public Vec2Int Resolution = (Vec2Int)Constants.DefaultResolution;

        public bool baseImageDirty = true;
        
        public Bitmap FallBack
        {
            get => fallback ??= new(256, 256);
        }

        public abstract void Render(Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();
        private static Vec2 ViewportToPosWithDrawingType(Camera cam, Vec2 size, Vec2 ViewportPos)
        {
            Vec2 maxIndex = size - Vec2.one;
            return cam.DrawMode switch
            {
                DrawingType.Wrapped => ViewportPos.Wrapped(Vec2.one) * maxIndex,
                DrawingType.Clamped => (ViewportPos.Clamped(Vec2.zero, Vec2.one) * maxIndex).Clamped(Vec2.zero, maxIndex),
                _ => new(0, 0),
            };
        }
        private void DrawBackground(Camera cam)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Vec2 backgroundSize = new(baseImage.GetLength(0), baseImage.GetLength(1));

            for (Vec2Int framePos = new(0,0); framePos.y < Resolution.y; framePos.Increment2D(Resolution.x))
            {
                Vec2 camViewport = cam.ScreenToCamViewport(framePos / ((Vec2)Resolution).GetDivideSafe());
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 global = cam.ViewportToGlobal(camViewport);
                Vec2 bgViewportPos = global / backgroundSize.GetDivideSafe();
                Vec2Int bgPos = (Vec2Int)ViewportToPosWithDrawingType(cam, backgroundSize, bgViewportPos);

                WriteColorToFrame(baseImage[bgPos.x, bgPos.y], framePos);
            }
        }
        public void RenderSprites(Camera camera, StageRenderInfo renderInfo)
        {
            Node camNode = new("CamNode", camera.parent.position, Vec2.one);
            Camera cam = camNode.AddComponent(camera.Clone());
            
            if (cam.zBuffer.GetLength(0) != Resolution.x || cam.zBuffer.GetLength(1) != Resolution.y)
                cam.zBuffer = new float[Resolution.x, Resolution.y];

            Array.Clear(cam.zBuffer);

            DrawBackground(cam);

            Node spriteNode = new("SpriteNode", Vec2.zero, Vec2.one);
            Sprite sprite = spriteNode.AddComponent<Sprite>();

            for (int i = 0; i < renderInfo.Count; ++i)
            {
                sprite.parent.position = renderInfo.spritePositions[i];
                sprite.ColorData = renderInfo.spriteColorData[i];
                sprite.size = renderInfo.spriteSizeVectors[i];
                sprite.camDistance = renderInfo.spriteCamDistances[i];
                RenderSprite(cam, sprite);
            }
        }

        private void RenderSprite(Camera cam, Sprite sprite)
        {
            //Bounding box on screen which fully captures sprite
            Vec2Int BB_Min = (Vec2Int)(cam.GlobalToScreenViewport(sprite.parent.position) * Resolution);
            Vec2Int BB_Max = (Vec2Int)(cam.GlobalToScreenViewport(sprite.parent.position) * Resolution);
            List<Vec2Int> corners = new()
            {
                (Vec2Int)(cam.GlobalToScreenViewport(sprite.parent.position + new Vec2(sprite.size.x, 0)) * Resolution),
                (Vec2Int)(cam.GlobalToScreenViewport(sprite.parent.position + new Vec2(0, sprite.size.y)) * Resolution),
                (Vec2Int)(cam.GlobalToScreenViewport(sprite.parent.position + sprite.size) * Resolution)
            };
            foreach (Vec2Int corner in corners)
            {
                BB_Min.x = Math.Min(corner.x, BB_Min.x);
                BB_Min.y = Math.Min(corner.y, BB_Min.y);
                BB_Max.x = Math.Max(corner.x + 1, BB_Max.x);
                BB_Max.y = Math.Max(corner.y + 1, BB_Max.y);
            }

            if (!((Vec2)BB_Min).IsWithinMaxExclusive(Vec2.zero, Resolution) ||
                !((Vec2)BB_Max).IsWithinMaxExclusive(Vec2.zero, Resolution)) return;

            for (Vec2Int framePos = BB_Min; framePos.y < BB_Max.y; framePos.Increment2D(BB_Max.x, BB_Min.x))
            {
                if (sprite.camDistance <= cam.zBuffer[framePos.x, framePos.y]) continue;

                Vec2 camViewport = cam.ScreenToCamViewport(framePos / ((Vec2)Resolution).GetDivideSafe());
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 spriteViewport = cam.ViewportToSpriteViewport(sprite, camViewport);
                if (!spriteViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2Int colorPos = sprite.ViewportToColorPos(spriteViewport);
                if (sprite.ColorData[colorPos.x, colorPos.y].A == 0) continue;

                if (sprite.ColorData[colorPos.x, colorPos.y].A == 255)
                    cam.zBuffer[framePos.x, framePos.y] = sprite.camDistance;

                WriteColorToFrame(sprite.ColorData[colorPos.x, colorPos.y], framePos);
            }
        }

        private void WriteColorToFrame(Color color, Vec2Int framePos)
        {
            int index = framePos.y * stride + (framePos.x * 3);

            int colorB = (int)((float)color.B / 255 * color.A);
            int colorG = (int)((float)color.G / 255 * color.A);
            int colorR = (int)((float)color.R / 255 * color.A);

            int frameB = (int)((float)frame[index + 0] / 255 * (255 - color.A));
            int frameG = (int)((float)frame[index + 1] / 255 * (255 - color.A));
            int frameR = (int)((float)frame[index + 2] / 255 * (255 - color.A));

            frame[index + 0] = (byte)(colorB + frameB);
            frame[index + 1] = (byte)(colorG + frameG);
            frame[index + 2] = (byte)(colorR + frameR);
        }
    }
    }

