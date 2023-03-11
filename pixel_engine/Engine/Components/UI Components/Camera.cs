using Newtonsoft.Json;
using pixel_renderer.ShapeDrawing;
using System;
using System.Linq;
using System.Numerics;
using System.Printing.IndexedProperties;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace pixel_renderer
{
    public class Camera : UIComponent
    {
        public CameraInfo? camInfo;
        public static Camera? First =>
            Runtime.Current.GetStage()?.GetAllComponents<Camera>().FirstOrDefault();
        public override void Draw(RendererBase renderer) =>
            camInfo?.Draw(renderer);
    }
}
