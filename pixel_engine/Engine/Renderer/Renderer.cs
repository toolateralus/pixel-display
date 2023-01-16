using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        public bool baseImageDirty = true;
        Color[,] baseImage = new Color[1,1];
        byte[] frame = Array.Empty<byte>();
        int stride = 0;
        Vec2Int size = new(256,256);

        public override void Dispose()
        {
            Array.Clear(frame);
        }
        public override void Draw(StageRenderInfo renderInfo)
        {
            if (baseImageDirty)
            {
                baseImage = CBit.ColorArrayFromBitmap(Runtime.Instance.GetStage().backgroundImage);
                baseImageDirty = false;
            }
            lock (frame)
            {
                stride = 4 * (size.x * 24 + 31) / 32;
                if (frame.Length != stride * size.y) frame = new byte[stride * size.y];
                IEnumerable<UIComponent> uiComponents = Runtime.Instance.GetStage().GetAllComponents<UIComponent>();
                foreach (UIComponent uiComponent in uiComponents.OrderBy(c => c.drawOrder))
                {
                    if (!uiComponent.Enabled) continue;
                    if (uiComponent is Camera) RenderSprites((Camera)uiComponent, renderInfo);
                }
            }
        }
        public void RenderSprites(Camera camera, StageRenderInfo renderInfo)
        {
            var node = Node.New;
            node.position = camera.parent.position;

            Camera cam = camera.Clone();
            cam.parent = node;

            if (cam.zBuffer.GetLength(0) != size.x || cam.zBuffer.GetLength(1) != size.y)
                cam.zBuffer = new float[size.x, size.y];

            Array.Clear(cam.zBuffer);

            DrawBackground(cam);

            for (int i = 0; i < renderInfo.Count; ++i)
            {
                var pos = renderInfo.spritePositions[i];
                var colorData = renderInfo.spriteColorData[i];
                Vec2Int size = new(colorData.GetLength(0), colorData.GetLength(1));
                var camDistance = renderInfo.spriteCamDistances[i];

                for (Vec2Int localPos = new(0,0); localPos.y < size.y; localPos.Increment2D(size.x))
                {
                    if (colorData[localPos.x, localPos.y].A == 0)
                        continue;

                    Vec2 camViewport = cam.GlobalToCamViewport(pos + localPos);
                    if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one))
                        continue;

                    Vec2Int framePos = (Vec2Int)(cam.CamToScreenViewport(camViewport) * this.size);
                    if (camDistance <= cam.zBuffer[framePos.x, framePos.y])
                        continue;

                    if (colorData[localPos.x, localPos.y].A == 255)
                        cam.zBuffer[framePos.x, framePos.y] = camDistance;

                    WriteColorToFrame(colorData[localPos.x, localPos.y], framePos);
                }
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
            frame[index + 1] = (byte)(colorG + frameB);
            frame[index + 2] = (byte)(colorR + frameB);
        }

        private void DrawBackground(Camera cam)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Vec2 bgSize = new(baseImage.GetLength(0), baseImage.GetLength(1));

            for (Vec2Int framePos = new(0,0); framePos.y < size.y; framePos.Increment2D(size.x))
            {
                Vec2 camViewport = cam.ScreenToCamViewport(framePos / ((Vec2)size).GetDivideSafe());
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 global = cam.CamViewportToGlobal(camViewport);
                Vec2 bgViewportPos = global / bgSize.GetDivideSafe();
                Vec2Int bgPos = (Vec2Int)BgViewportToBgPos(cam, bgSize, bgViewportPos);
                WriteColorToFrame(baseImage[bgPos.x, bgPos.y], framePos);
            }
        }

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

        public override void Render(System.Windows.Controls.Image destination)
        {
            destination.Source = BitmapSource.Create(
                size.x, size.y, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
                frame, stride);
        }
    }
}