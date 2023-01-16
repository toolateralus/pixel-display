using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Bitmap = System.Drawing.Bitmap;

namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        private Bitmap? renderTexture = null; 
        private Bitmap? _background;
        byte[] frameBuffer = System.Array.Empty<byte>();
        int frameStride = 0;
        Vec2Int frameSize = new();
        public Bitmap Background
        {
            get
            {
                _background ??= Runtime.Instance.GetStage().backgroundImage;
                return _background;
            }
        }

        public override void Dispose()
        {
            if (Background is not null)
            {
                renderTexture = (Bitmap)Background.Clone();
                return;
            }
            renderTexture = (Bitmap)FallBack.Clone();
        }
        public override Bitmap Draw(StageRenderInfo renderInfo)
        {
            IEnumerable<UIComponent> uiComponents = Runtime.Instance.GetStage().GetAllComponents<UIComponent>();


            foreach (UIComponent uiComponent in uiComponents.OrderBy(c => c.drawOrder))
            {
                if (!uiComponent.Enabled ) continue;
                if (uiComponent as Camera != null) RenderSprites(uiComponent as Camera, renderInfo);
            }
                
            return renderTexture;
        }
        public void RenderSprites(Camera camera, StageRenderInfo renderInfo)
        {
            if (renderTexture == null) return;

            var node = Node.New;
            node.position = camera.parent.position;

            Camera cam = camera.Clone();
            cam.parent = node;
            BitmapData bmd = UpdateFrameInfo();

            if (cam.zBuffer.GetLength(0) != frameSize.x || cam.zBuffer.GetLength(1) != frameSize.y)
                cam.zBuffer = new float[frameSize.x, frameSize.y];

            Array.Clear(cam.zBuffer);

            DrawBackground(cam);

            for (int i = 0; i < renderInfo.Count; ++i)
            {
                var pos = renderInfo.spritePositions[i];
                var colorData = renderInfo.spriteColorData[i];
                Vec2Int size = new(colorData.GetLength(0), colorData.GetLength(1));
                var camDistance = renderInfo.spriteCamDistances[i];

                for (Vec2Int localPos = new(); localPos.y < size.y; localPos.Increment2D(size.x))
                {
                    if (colorData[localPos.x, localPos.y].A == 0)
                        continue;

                    Vec2 camViewport = cam.GlobalToCamViewport(pos + localPos);
                    if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one))
                        continue;

                    Vec2Int framePos = (Vec2Int)(cam.CamToScreenViewport(camViewport) * frameSize);
                    if (camDistance <= cam.zBuffer[framePos.x, framePos.y])
                        continue;

                    if (colorData[localPos.x, localPos.y].A == 255)
                        cam.zBuffer[framePos.x, framePos.y] = camDistance;

                    WriteColorToFrame(colorData[localPos.x, localPos.y], framePos);
                }
            }
            lock (frameBuffer)
            {
                Marshal.Copy(frameBuffer, 0, bmd.Scan0, frameBuffer.Length);
                renderTexture.UnlockBits(bmd);
            }
        }

        private BitmapData UpdateFrameInfo()
        {
            var bmd = renderTexture.LockBits(
                new(0, 0, renderTexture.Width, renderTexture.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                renderTexture.PixelFormat
                );

            bool shouldResizeFrame = false;

            if (bmd.Width != frameSize.x || bmd.Height != frameSize.y)
            {
                frameSize.x = bmd.Width;
                frameSize.y = bmd.Height;
                shouldResizeFrame = true;
            }

            if (frameStride != bmd.Stride)
            {
                frameStride = bmd.Stride;
                shouldResizeFrame = true;
            }

            if (shouldResizeFrame) frameBuffer = new byte[bmd.Stride * bmd.Height];
            return bmd;
        }

        private void WriteColorToFrame(System.Drawing.Color color, Vec2Int framePos)
        {
            int frameBufferIndex = framePos.y * frameStride + (framePos.x * 3);

            int colorB = (int)((float)color.B / 255 * color.A);
            int colorG = (int)((float)color.G / 255 * color.A);
            int colorR = (int)((float)color.R / 255 * color.A);

            int frameB = (int)((float)frameBuffer[frameBufferIndex + 0] / 255 * (255 - color.A));
            int frameG = (int)((float)frameBuffer[frameBufferIndex + 1] / 255 * (255 - color.A));
            int frameR = (int)((float)frameBuffer[frameBufferIndex + 2] / 255 * (255 - color.A));

            frameBuffer[frameBufferIndex + 0] = (byte)(colorB + frameB);
            frameBuffer[frameBufferIndex + 1] = (byte)(colorG + frameB);
            frameBuffer[frameBufferIndex + 2] = (byte)(colorR + frameB);
        }

        private void DrawBackground(Camera cam)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Bitmap bg = Background;
            Vec2 bgSize = new(bg.Width, bg.Height);

            for (Vec2Int targetPos = new(0,0); targetPos.y < frameSize.y; targetPos.Increment2D(frameSize.x))
            {
                Vec2 camViewport = cam.ScreenToCamViewport(targetPos / ((Vec2)frameSize).GetDivideSafe());
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 global = cam.CamViewportToGlobal(camViewport);
                Vec2 bgViewportPos = global / bgSize.GetDivideSafe();
                Vec2Int bgPos = (Vec2Int)BgViewportToBgPos(cam, bgSize, bgViewportPos);
                WriteColorToFrame(bg.GetPixel(bgPos.x, bgPos.y), targetPos);
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

        public override void Render(Image destination) => CBit.Render(renderTexture, destination);
    }
}