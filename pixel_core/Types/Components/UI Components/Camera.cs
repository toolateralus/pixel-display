﻿using System.Linq;

namespace pixel_core
{
    public class Camera : UIComponent
    {
        public CameraInfo? camInfo;
        public static Camera? First =>
            Interop.GetStage()?.GetAllComponents<Camera>().FirstOrDefault();
        public override void Draw(RendererBase renderer) => camInfo?.Draw(renderer);
        public override void Dispose()
        {
            camInfo = null;
        }
    }
}
