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
        public override Bitmap Draw()
        {
            IEnumerable<Camera> cams = Runtime.Instance.GetStage().GetAllComponents<Camera>();
            IEnumerable<Sprite> sprites = Runtime.Instance.GetStage().GetAllComponents<Sprite>();
            foreach (Camera cam in cams)
                if(cam.Enabled) RenderSprites(cam, sprites);
            return renderTexture;
        }
        public void RenderSprites(Camera cam, IEnumerable<Sprite> sprites)
        {
            if (renderTexture == null) return;

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

            foreach (Sprite sprite in sprites)
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        Vec2 camViewport = cam.GlobalToCamViewport(sprite.parent.position + new Vec2(x, y));
                        if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                        Vec2 screenPos = cam.CamToScreenViewport(camViewport) * bmpSize;

                        if (sprite.camDistance <= cam.zBuffer[(int)screenPos.x, (int)screenPos.y]) continue;
                        cam.zBuffer[(int)screenPos.x, (int)screenPos.y] = sprite.camDistance;
                        SetPixelColor(bmd, sprite.ColorData[x,y], screenPos);
                    }
            Marshal.Copy(frameBuffer, 0, bmd.Scan0, frameBuffer.Length);
            renderTexture.UnlockBits(bmd);
        }

        private void SetPixelColor(BitmapData bmd, System.Drawing.Color color, Vec2 screenPos)
        {
            int frameBufferIndex = (int)screenPos.y * bmd.Stride + ((int)screenPos.x * 3);

            frameBuffer[frameBufferIndex + 0] = color.R;
            frameBuffer[frameBufferIndex + 1] = color.G;
            frameBuffer[frameBufferIndex + 2] = color.B;
        }

        private void DrawBackground(Camera cam, BitmapData bmd)
        {
            if (cam.DrawMode is DrawingType.None) return;

            Bitmap bg = Background;
            Vec2 bgSize = new(bg.Width, bg.Height);
            Vec2 bmpSize = new(bmd.Width, bmd.Height);

            for (int x = 0; x < bmd.Width; x++)
            {
                for (int y = 0; y < bmd.Height; y++)
                {
                    Vec2 camViewport = cam.ScreenToCamViewport(new Vec2(x, y) / bmpSize.GetDivideSafe());
                    if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                    Vec2 global = cam.CamViewportToGlobal(camViewport);
                    Vec2 bgViewport = global / bgSize.GetDivideSafe();

                    if (cam.DrawMode == DrawingType.Wrapped)
                        DrawWrapped(bmd, bg, bgSize, x, y, bgViewport);

                    if (cam.DrawMode == DrawingType.Clamped)
                        DrawClamped(bmd, bg, bgSize, x, y, bmpSize, bgViewport);
                }
            }
        }
        public void DrawClamped(BitmapData bmd, Bitmap background, Vec2 bgSize, int x, int y, Vec2 bmpSize, Vec2 bgViewport)
        {
            bgViewport.Clamp(Vec2.zero, Vec2.one);
            Vec2 bgPos = (bgViewport * (bgSize - Vec2.one)).Clamped(Vec2.zero, bmpSize);
            SetPixelColor(bmd, background.GetPixel((int)bgPos.x, (int)bgPos.y), new Vec2(x, y));
        }
        public void DrawWrapped(BitmapData bmd, Bitmap background, Vec2 bgSize, int x, int y, Vec2 bgViewport)
        {
            bgViewport += new Vec2(1, 1);
            Vec2 wrappedBgViewport = new(bgViewport.x - (int)bgViewport.x, bgViewport.y - (int)bgViewport.y);
            Vec2 bgPos = wrappedBgViewport * bgSize;
            SetPixelColor(bmd, background.GetPixel((int)bgPos.x, (int)bgPos.y), new Vec2(x, y));
        }
        public override void Render(Image destination) => CBit.Render(renderTexture, destination);
    }
}