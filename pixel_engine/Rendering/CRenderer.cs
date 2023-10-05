using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Pixel.CBit;

namespace Pixel
{
    public class CRenderer : RendererBase
    {
        public override void Dispose()
        {
            if (frameBuffer.Count < 3)
                return;
            
            Array.Clear(frameBuffer[0]);
            ByteArrayPool.Shared.Return(frameBuffer[0]);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Draw(StageRenderInfo renderInfo)
        {
            var Resolution = this.Resolution;

            if (Runtime.Current.GetStage() is not Stage stage)
                return;

            if (baseImageDirty)
            {
                baseImage = stage.GetBackground();
                baseImageDirty = false;
            }

            stride = 4 * ((int)Resolution.X * 24 + 31) / 32;

            while (frameBuffer.Count < 3)
                frameBuffer.Add(ByteArrayPool.Shared.Rent((int)(stride * Resolution.Y)));

            IEnumerable<UIComponent> uiComponents = stage.GetAllComponents<UIComponent>();
            foreach (UIComponent uiComponent in uiComponents.OrderBy(c => c.drawOrder))
                if (uiComponent.Enabled)
                    uiComponent.Draw(this);

            var first = frameBuffer[0];
            frameBuffer[0] = frameBuffer[2];
            frameBuffer[2] = frameBuffer[1];
            frameBuffer[1] = first;

        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Render(Image output)
        {
            if (stride != 0 && frameBuffer.Count > 2)
                RenderFromFrame(frameBuffer[1], stride, Resolution);
        }
    }
}