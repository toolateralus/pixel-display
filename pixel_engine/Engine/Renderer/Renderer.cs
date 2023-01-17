using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        public override void Dispose() => Array.Clear(frame);
        public override void Draw(StageRenderInfo renderInfo)
        {
            if (baseImageDirty)
            {
                baseImage = CBit.ColorArrayFromBitmap(Runtime.Instance.GetStage().backgroundImage);
                baseImageDirty = false;
            }
            lock (frame)
            {
                stride = 4 * (Resolution.x * 24 + 31) / 32;
                if (frame.Length != stride * Resolution.y) frame = new byte[stride * Resolution.y];
                
                IEnumerable<UIComponent> uiComponents = Runtime.Instance.GetStage().GetAllComponents<UIComponent>();

                foreach (UIComponent uiComponent in uiComponents.OrderBy(c => c.drawOrder))
                    if (uiComponent.Enabled && uiComponent is Camera camera) 
                        RenderSprites(camera, renderInfo);
            }
        }

        public override void Render(System.Windows.Controls.Image output)
        {
            output.Source = BitmapSource.Create(
                Resolution.x, Resolution.y, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
                frame, stride);
        }
    }
}