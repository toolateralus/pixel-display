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

            var bmd = renderTexture.LockBits(
                new(0, 0, renderTexture.Width, renderTexture.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                renderTexture.PixelFormat
                );

            bool shouldResize =
                bmd.Height != cam.zBuffer.GetLength(1) ||
                bmd.Width != cam.zBuffer.GetLength(0);

            if (shouldResize)
            {
                cam.zBuffer = new float[bmd.Width, bmd.Height];
                frameBuffer = new byte[bmd.Stride * bmd.Height];
            }

            System.Array.Clear(cam.zBuffer);

            DrawBackground(cam, bmd);

            Vec2 bmpSize = new Vec2(bmd.Width, bmd.Height);

            for (int i = 0; i < renderInfo.Count; ++i)
            {
                var pos = renderInfo.spritePositions[i];
                var colorData = renderInfo.spriteColorData[i];
                var size = new Vec2(colorData.GetLength(0) , colorData.GetLength(1));
                var camDistance = renderInfo.spriteCamDistances[i];

                for (Vec2Int localPos = new(); localPos.y < size.y; localPos.Increment2D((int)size.x))
                {
                    Vec2 camViewport = cam.GlobalToCamViewport(pos + localPos);

                    if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) 
                        continue;

                    Vec2 screenPos = cam.CamToScreenViewport(camViewport) * bmpSize;

                    if (camDistance <= cam.zBuffer[(int)screenPos.x, (int)screenPos.y])
                        continue;

                    cam.zBuffer[(int)screenPos.x, (int)screenPos.y] = camDistance;

                    SetPixelColor(bmd, colorData[localPos.x, localPos.y], screenPos);
                }
            }
            lock (frameBuffer)
            {
                Marshal.Copy(frameBuffer, 0, bmd.Scan0, frameBuffer.Length);
            }
            renderTexture.UnlockBits(bmd);
        }

        private void SetPixelColor(BitmapData bmd, System.Drawing.Color color, Vec2 screenPos)
        {
            int frameBufferIndex = (int)screenPos.y * bmd.Stride + ((int)screenPos.x * 3);

            frameBuffer[frameBufferIndex + 0] = color.B;
            frameBuffer[frameBufferIndex + 1] = color.G;
            frameBuffer[frameBufferIndex + 2] = color.R;
        }

        private void DrawBackground(Camera cam, BitmapData bmd)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Bitmap bg = Background;
            Vec2 bgSize = new(bg.Width, bg.Height);
            Vec2 bmpSize = new(bmd.Width, bmd.Height);

            for (Vec2Int screenPos = new(0,0); screenPos.y < bmd.Height; screenPos.Increment2D(bmd.Width))
            {
                Vec2 camViewport = cam.ScreenToCamViewport(screenPos / bmpSize.GetDivideSafe());
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 global = cam.CamViewportToGlobal(camViewport);
                Vec2 bgViewportPos = global / bgSize.GetDivideSafe();
                Vec2 bgPos = BgViewportToBgPos(cam, bgSize, bgViewportPos);
                SetPixelColor(bmd, bg.GetPixel((int)bgPos.x, (int)bgPos.y), screenPos);
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