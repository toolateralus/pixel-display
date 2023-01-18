using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using static pixel_renderer.CBit; 
namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        public override void Dispose() => Array.Clear(frame);
        public override void Draw(StageRenderInfo renderInfo)
        {
            if (baseImageDirty)
            {
                baseImage = ColorArrayFromBitmap(Runtime.Instance.GetStage().backgroundImage);
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
        public override void Render(Image output) => RenderFromFrame(frame, stride, Resolution, output);

    }
}