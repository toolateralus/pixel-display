using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using static pixel_renderer.CBit; 
namespace pixel_renderer
{
    public class CRenderer : RendererBase
    {
       
        public override void Dispose() => Array.Clear(frame);
        public override void Draw(StageRenderInfo renderInfo)
        {
            if (Runtime.Instance.GetStage() is not Stage stage) return;
            if (baseImageDirty)
            {
                baseImage = ColorArrayFromBitmap(stage.InitializedBackground);
                baseImageDirty = false;
            }

            // TODO: restore the original synchronous rendering, just seeing if this would be faster.
            Task DrawTask = new(delegate
            {
                stride = 4 * ((int)Resolution.x * 24 + 31) / 32;

                if (frame.Length != stride * Resolution.y)
                    frame = new byte[stride * (int)Resolution.y];

                //TODO: find a way to use this without causing an invalid cast exception
                //IEnumerable<UIComponent> uiComponents = stage.GetAllComponents<UIComponent>();

                List<Component> componentsFound = new();

                foreach (var node in stage.nodes)
                {
                    var result = componentsFound.Concat(node.ComponentsList);

                    if (result != null)
                        componentsFound = result.ToList();
                }

                List<UIComponent> uiComponents = new();

                foreach (var comp in componentsFound)
                    if (comp is UIComponent uiComp)
                        uiComponents.Add(uiComp);


                // TODO : Fix invalid cast exception from Component to UIComponent, likely a JSON problem
                foreach (UIComponent uiComponent in uiComponents.OrderBy(c => c.drawOrder))
                    if (uiComponent.Enabled && uiComponent is Camera camera)
                        RenderSprites(camera, renderInfo);
            });
            DrawTask.RunSynchronously();
        }

        public override void Render(Image output)
        {
            if(stride != 0)
                RenderFromFrame(frame, stride, Resolution, output);
        }
    }
}