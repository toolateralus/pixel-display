using System;
using System.Linq;

namespace Pixel
{
    public class Camera : UIComponent
    {
        public CameraInfo? camInfo;
        private static WeakReference<Camera> refFirst;
        public static Camera? First
        {
            set 
            {
                refFirst = new(value);
            } 
            get
            {
                if (refFirst != null && refFirst.TryGetTarget(out var val))
                    return val;

                if (Interop.Stage?.GetAllComponents<Camera>().FirstOrDefault() is Camera cam)
                {
                    refFirst = new(cam);
                    return cam;
                }
                else refFirst = null;

                return Interop.Stage?.GetAllComponents<Camera>().FirstOrDefault();
            }
        }

        public override void Draw(RendererBase renderer) => camInfo?.Draw(renderer);
        public override void Dispose()
        {
            camInfo = null;
        }
    }
}
