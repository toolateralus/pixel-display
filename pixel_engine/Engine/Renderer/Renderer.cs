using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
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
        public override void Render(Image destination)
        {
            if(renderTexture is null) return;

            CBit.WriteOnlyBitmapData(renderTexture, out var bmd);
            IntPtr scan0 = bmd.Scan0;
            Marshal.Copy(frameBuffer, 0, bmd.Scan0, frameBuffer.Length);
            renderTexture.UnlockBits(bmd); 
            CBit.DeleteObject(scan0);

            CBit.Render(renderTexture, destination);
        }
        public void RenderSprites(Camera cam, IEnumerable<Sprite> sprites)
        {
           
            if (renderTexture == null) return;


            int height = renderTexture.Height;
            int width = renderTexture.Width;
            int stride = 4 * (width * 24 + 31) / 32);

            bool shouldResize =
                height != cam.zBuffer.GetLength(1) ||
                width != cam.zBuffer.GetLength(0);

            if (shouldResize)
            {
                cam.zBuffer = new float[width, height];
                frameBuffer = new byte[stride * height];
            }

            System.Array.Clear(cam.zBuffer);

            DrawBackground(cam, height, width, stride);
            Vec2 bmpSize = new Vec2(width, height);

            foreach (Sprite sprite in sprites)
                for (Vec2Int spritePos = new(); spritePos.y < sprite.size.y; spritePos.Increment2D((int)sprite.size.x))
                {
                    Vec2 camViewport = cam.GlobalToCamViewport(sprite.parent.position + spritePos);
                    if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                    Vec2 screenPos = cam.CamToScreenViewport(camViewport) * bmpSize;

                    if (sprite.camDistance <= cam.zBuffer[(int)screenPos.x, (int)screenPos.y]) continue;
                    cam.zBuffer[(int)screenPos.x, (int)screenPos.y] = sprite.camDistance;
                    SetPixelColor(width, height, stride, sprite.ColorData[spritePos.x, spritePos.y], screenPos);
                }
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

    }
}