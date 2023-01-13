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
            foreach (SpriteCamera uiComponent in Runtime.Instance.GetStage().GetAllComponents<SpriteCamera>())
            {
                if(uiComponent.Enabled == false) continue;
                    uiComponent.Draw(renderTexture);
            }
            return renderTexture;
        }
        public override void Render(Image destination) => CBit.Render(ref renderTexture, destination);
    }
}