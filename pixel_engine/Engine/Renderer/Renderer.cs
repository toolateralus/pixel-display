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
            IEnumerable<SpriteCamera> cams = Runtime.Instance.GetStage().GetAllComponents<SpriteCamera>();
            foreach (SpriteCamera cam in cams)
                if(cam.Enabled) cam.Draw(renderTexture);
            return renderTexture;
        }
        public override void Render(Image destination) => CBit.Render(renderTexture, destination);
    }
}