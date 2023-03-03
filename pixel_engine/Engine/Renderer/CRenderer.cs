﻿using System;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Draw(StageRenderInfo renderInfo)
        {

            if (Runtime.Current.GetStage() is not Stage stage) return;
            if (baseImageDirty)
            {
                baseImage = stage.GetBackground(); 
                baseImageDirty = false;
            }

            stride = 4 * ((int)Resolution.X * 24 + 31) / 32;

            if (frame.Length != stride * Resolution.Y)
                frame = new byte[stride * (int)Resolution.Y];


            List<Component> componentsFound = new();
            lock(stage.nodes)
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


            foreach (UIComponent uiComponent in uiComponents.OrderBy(c => c.drawOrder))
                if (uiComponent.IsActive && uiComponent is Camera camera)
                    RenderCamera(camera, renderInfo, Resolution);
         
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Render(Image output)
        {
            if(stride != 0)
                RenderFromFrame(frame, stride, Resolution, output);
        }
    }
}