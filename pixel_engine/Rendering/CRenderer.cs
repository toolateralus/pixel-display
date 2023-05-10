using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Pixel.CBit;

namespace Pixel
{
    public class CRenderer : RendererBase
    {
        public override void Dispose()
        {
            if (frameBuffer.Count < 3)
                return;

            var last = frameBuffer[2];
            var first = frameBuffer[0];

            frameBuffer[0] = last;
            frameBuffer[2] = first;
            frameBuffer[1] = frameBuffer[0];

            ByteArrayPool.Shared.Return(frameBuffer[0]);
            Array.Clear(frameBuffer[0]);
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

           
         
            Array.Copy(frameBuffer[0], frameBuffer[1], frameBuffer[0].Length);

        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Render(System.Windows.Controls.Image output)
        {
            if (stride != 0)
                RenderFromFrame(frameBuffer[0], stride, Resolution, output);
        }
    }
}