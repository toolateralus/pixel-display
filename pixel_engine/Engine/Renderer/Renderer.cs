using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;

namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        private Bitmap? renderTexture = null; 
        private volatile Bitmap? _background;
        byte[] frameBuffer = System.Array.Empty<byte>();
        Task? frameBufferTask = null;
        public bool HasRenderTexture => renderTexture != null;
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
        public override Bitmap Draw()
        {
            IEnumerable<Camera> cams = Runtime.Instance.GetStage().GetAllComponents<Camera>();
            IEnumerable<Sprite> sprites = Runtime.Instance.GetStage().GetAllComponents<Sprite>();
            foreach (Camera cam in cams)
            {
                if (cam.Enabled)
                {
                    lock(renderTexture) 
                        RenderSprites(cam, sprites);
                }
            }
            return renderTexture;
        }
        public async override void Render(Image destination)
        {
            if (renderTexture is null) return;
            CBit.WriteOnlyBitmapData(renderTexture, out var bmd);
            IntPtr scan0 = bmd.Scan0;
            await WaitForFramebufferTask(WriteFrameToBitmap(bmd));
            renderTexture.UnlockBits(bmd);
            CBit.DeleteObject(scan0);
            CBit.Render(renderTexture, destination);

            return;
        }

        private async Task WaitForFramebufferTask(Task task)
        {
            if (frameBufferTask != null) await frameBufferTask;
            frameBufferTask = task;
            await frameBufferTask;
            frameBufferTask = null;
        }

        private Task WriteFrameToBitmap(BitmapData bmd)
        {
            Marshal.Copy(frameBuffer, 0, bmd.Scan0, frameBuffer.Length);
            return Task.CompletedTask;
        }

        public async void RenderSprites(Camera cam, IEnumerable<Sprite> sprites)
        {
            if (renderTexture == null) return;

            int height = renderTexture.Height;
            int width = renderTexture.Width;
            int stride = 4 * (width * 24 + 31) / 32;
            Vec2Int bmpSize = new (width, height);

            bool shouldResize =
                height != cam.zBuffer.GetLength(1) ||
                width != cam.zBuffer.GetLength(0);

            if (shouldResize)
            {
                cam.zBuffer = new float[width, height];
                frameBuffer = new byte[stride * height];
            }

            System.Array.Clear(cam.zBuffer);

            DrawBackground(cam, bmpSize, stride);

            foreach (Sprite sprite in sprites)
                for (Vec2Int spritePos = new(); spritePos.y < sprite.size.y; spritePos.Increment2D((int)sprite.size.x))
                {
                    Vec2 camViewport = cam.GlobalToCamViewport(sprite.parent.position + spritePos);
                    if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                    Vec2Int screenPos = (Vec2Int)(cam.CamToScreenViewport(camViewport) * bmpSize);

                    if (sprite.camDistance <= cam.zBuffer[screenPos.x, screenPos.y]) continue;
                    cam.zBuffer[screenPos.x, screenPos.y] = sprite.camDistance;
                    Task task = SetPixelColor(stride, sprite.ColorData[spritePos.x, spritePos.y], screenPos);
                    await WaitForFramebufferTask(task);
                }
        }

        private Task SetPixelColor(int stride, System.Drawing.Color color, Vec2Int screenPos)
        {
            int frameBufferIndex = screenPos.y * stride + (screenPos.x * 3);

            frameBuffer[frameBufferIndex + 0] = color.B;
            frameBuffer[frameBufferIndex + 1] = color.G;
            frameBuffer[frameBufferIndex + 2] = color.R;

            return Task.CompletedTask;
        }
        private void DrawBackground(Camera cam, Vec2Int bmpSize, int stride)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Bitmap background = Background;
            Vec2 bgSize = new(background.Width, background.Height);

            for (Vec2Int screenPos = new(0,0); screenPos.y < bmpSize.x; screenPos.Increment2D(bmpSize.x))
            {
                Vec2 camViewport = cam.ScreenToCamViewport((Vec2)screenPos / bmpSize.GetDivideSafe());
                if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                Vec2 global = cam.CamViewportToGlobal(camViewport);
                Vec2 bgViewportPos = global / bgSize.GetDivideSafe();
                Vec2Int bgPos = (Vec2Int)BgViewportToBgPos(cam, bgSize, bgViewportPos);
                SetPixelColor(stride, background.GetPixel(bgPos.x, bgPos.y), screenPos);
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

    }
}