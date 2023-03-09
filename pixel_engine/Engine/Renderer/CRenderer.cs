using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using static pixel_renderer.CBit; 
namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
        public override void Dispose() => Array.Clear(frame);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Draw(StageRenderInfo renderInfo)
        {

            var Resolution = this.Resolution;

            if (Runtime.Current.GetStage() is not Stage stage) return;
            if (baseImageDirty)
            {
                baseImage = stage.GetBackground(); 
                baseImageDirty = false;
            }

            stride = 4 * ((int)Resolution.X * 24 + 31) / 32;

            if (frame.Length != stride * Resolution.Y)
                frame = new byte[stride * (int)Resolution.Y];

            IEnumerable<UIComponent> uiComponents = stage.GetAllComponents<UIComponent>().AsParallel();

            foreach (UIComponent uiComponent in uiComponents.OrderBy(c => c.drawOrder)) 
                if (uiComponent.IsActive)
                    uiComponent.Draw(this);

            if (latestFrame.Length != frame.Length)
                latestFrame = new byte[frame.Length];

            Array.Copy(frame, latestFrame, frame.Length);

        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Render(System.Windows.Controls.Image output)
        {
            if(stride != 0)
                RenderFromFrame(frame, stride, Resolution, output);
        }
    }
}