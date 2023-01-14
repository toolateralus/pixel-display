using System.Collections.Generic;
    using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        private Bitmap? renderTexture = null; 
        private Bitmap? _background;
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

            bool shouldResize =
                renderTexture.Height != cam.zBuffer.GetLength(1) ||
                renderTexture.Width != cam.zBuffer.GetLength(0);

            if (shouldResize)
                cam.zBuffer = new float[renderTexture.Width, renderTexture.Height];

            System.Array.Clear(cam.zBuffer);

            DrawBackground(cam);

            Vec2 bmpSize = new Vec2(renderTexture.Width, renderTexture.Height);

            foreach (Sprite sprite in sprites)
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        Vec2 camViewport = cam.GlobalToCamViewport(sprite.parent.position + new Vec2(x, y));
                        if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                        Vec2 screenPos = cam.CamToScreenViewport(camViewport) * bmpSize;

                        if (sprite.camDistance <= cam.zBuffer[(int)screenPos.x, (int)screenPos.y]) continue;
                        cam.zBuffer[(int)screenPos.x, (int)screenPos.y] = sprite.camDistance;

                        renderTexture.SetPixel((int)screenPos.x, (int)screenPos.y, sprite.ColorData[x, y]);
                    }
        }

        private void DrawBackground(Camera cam)
        {
            if (cam.DrawMode is DrawingType.None) return;
            if (renderTexture == null) return;

            Bitmap bg = Background;
            Vec2 bgSize = new(bg.Width, bg.Height);
            Vec2 bmpSize = new(renderTexture.Width, renderTexture.Height);

            for (int x = 0; x < renderTexture.Width; x++)
            {
                for (int y = 0; y < renderTexture.Height; y++)
                {
                    Vec2 camViewport = cam.ScreenToCamViewport(new Vec2(x, y) / bmpSize.GetDivideSafe());
                    if (!camViewport.IsWithinMaxExclusive(Vec2.zero, Vec2.one)) continue;

                    Vec2 global = cam.CamViewportToGlobal(camViewport);
                    Vec2 bgViewport = global / bgSize.GetDivideSafe();

                    if (cam.DrawMode == DrawingType.Wrapped)
                        DrawWrapped(renderTexture, bg, ref bgSize, ref x, ref y, ref bgViewport);

                    if (cam.DrawMode == DrawingType.Clamped)
                        DrawClamped(renderTexture, bg, ref bgSize, ref x, ref y, ref bmpSize, ref bgViewport);
                }
            }
        }
        public static void DrawClamped(Bitmap bmp, Bitmap background, ref Vec2 bgSize, ref int x, ref int y, ref Vec2 bmpSize, ref Vec2 bgViewport)
        {
            bgViewport.Clamp(Vec2.zero, Vec2.one);
            Vec2 bgPos = (bgViewport * (bgSize - Vec2.one)).Clamped(Vec2.zero, bmpSize);
            bmp.SetPixel(x, y, background.GetPixel((int)bgPos.x, (int)bgPos.y));
        }
        public static void DrawWrapped(Bitmap bmp, Bitmap background, ref Vec2 bgSize, ref int x, ref int y, ref Vec2 bgViewport)
        {
            bgViewport += new Vec2(1, 1);
            Vec2 wrappedBgViewport = new(bgViewport.x - (int)bgViewport.x, bgViewport.y - (int)bgViewport.y);
            Vec2 bgPos = wrappedBgViewport * bgSize;
            bmp.SetPixel(x, y, background.GetPixel((int)bgPos.x, (int)bgPos.y));
        }
        public override void Render(Image destination) => CBit.Render(renderTexture, destination);
    }
}