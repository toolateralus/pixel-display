namespace pixel_renderer
{
    using pixel_renderer.Engine.Renderer;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

    public class CRenderer : RendererBase
    {
        private Bitmap? bmp_cached = null; 
        private IEnumerable<Sprite>? sprites_cached = null;
        public SpriteCamera cam;

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
                bmp_cached = (Bitmap)Background.Clone();
                return;
            }
            bmp_cached = (Bitmap)FallBack.Clone();
        }

        public override Bitmap Draw()
        {
            sprites_cached = Runtime.Instance.GetStage().GetSprites();
            if (cam == null)
            {
                Vec2 bmpSize = new Vec2(bmp_cached.Width, bmp_cached.Height);
                cam = new SpriteCamera(bmpSize, bmpSize / 2);
            }
            cam.Draw(sprites_cached.ToList(), bmp_cached);
            return bmp_cached;
        }
        public override void Render(Image destination) => CBit.Render(ref bmp_cached, destination);
    }
}