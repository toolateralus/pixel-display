namespace pixel_renderer
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;
    using Color = System.Drawing.Color;

    public abstract class  RendererBase 
    {
        public Vec2Int resolution = new(256, 256);
        private protected Bitmap fallback;
        public bool baseImageDirty = true;
        private protected Color[,] baseImage = new Color[1,1];
        private protected byte[] frame = Array.Empty<byte>();
        private protected int stride = 0;
        public Bitmap FallBack
        {
            get => fallback ??= new(256, 256);
        }
        public abstract void Render(Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();
        private static Vec2 BgViewportToBgPos(Camera cam, Vec2 bgSize, Vec2 bgViewportPos)
        {
            Vec2 maxIndex = bgSize - Vec2.one;
            return cam.DrawMode switch
            {
                DrawingType.Wrapped => bgViewportPos.Wrapped(Vec2.one) * maxIndex,
                DrawingType.Clamped => (bgViewportPos.Clamped(Vec2.zero, Vec2.one) * maxIndex).Clamped(Vec2.zero, maxIndex),
                _ => new(0, 0),
            };
        }
        private void DrawBackground(Camera cam)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Vec2 bgSize = new(baseImage.GetLength(0), baseImage.GetLength(1));

            for (Vec2Int framePos = new(0,0); framePos.y < resolution.y; framePos.Increment2D(resolution.x))
            {
                Vec2 camViewport = cam.ScreenToCamViewport(framePos / ((Vec2)resolution).GetDivideSafe());
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 global = cam.CamViewportToGlobal(camViewport);
                Vec2 bgViewportPos = global / bgSize.GetDivideSafe();
                Vec2Int bgPos = (Vec2Int)BgViewportToBgPos(cam, bgSize, bgViewportPos);
                WriteColorToFrame(baseImage[bgPos.x, bgPos.y], framePos);
            }
        }
        public void RenderSprites(Camera camera, StageRenderInfo renderInfo)
        {
            var camNode = Node.New;
            camNode.position = camera.parent.position;

            Camera cam = camera.Clone();
            cam.parent = camNode;

            if (cam.zBuffer.GetLength(0) != resolution.x || cam.zBuffer.GetLength(1) != resolution.y)
                cam.zBuffer = new float[resolution.x, resolution.y];

            Array.Clear(cam.zBuffer);

            DrawBackground(cam);

            Node spriteNode = Node.New;
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

            for (Vec2Int localPos = new(0, 0); localPos.y < sprite.size.y; localPos.Increment2D((int)sprite.size.x))
            {
                if (localPos.x >= sprite.ColorData.GetLength(0)) continue;
                if (localPos.y >= sprite.ColorData.GetLength(1)) continue;
                if (sprite.ColorData[localPos.x, localPos.y].A == 0)
                    continue;

                Vec2 camViewport = cam.GlobalToCamViewport(sprite.parent.position + localPos);
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one))
                    continue;

                Vec2Int framePos = (Vec2Int)(cam.CamToScreenViewport(camViewport) * resolution);
                if (sprite.camDistance <= cam.zBuffer[framePos.x, framePos.y])
                    continue;

                if (sprite.ColorData[localPos.x, localPos.y].A == 255)
                    cam.zBuffer[framePos.x, framePos.y] = sprite.camDistance;

                WriteColorToFrame(sprite.ColorData[localPos.x, localPos.y], framePos);
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

