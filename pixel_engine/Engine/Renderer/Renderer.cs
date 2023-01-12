    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        private Bitmap? bmp_cached = null; 
        private IEnumerable<Sprite>? sprites_cached = null;

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
            foreach (SpriteCamera cam in Runtime.Instance.GetStage().GetAllComponents<SpriteCamera>())
            {
                if(cam.Enabled == false) continue;
                cam.Draw(bmp_cached);
            }
            return bmp_cached;
        }
        public override void Render(Image destination) => CBit.Render(ref bmp_cached, destination);
    }
}