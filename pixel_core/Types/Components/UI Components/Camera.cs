using System.Linq;

namespace Pixel
{
    public class Camera : UIComponent
    {
        public CameraInfo? camInfo;
        public static Camera? First =>
            Interop.            Stage?.GetAllComponents<Camera>().FirstOrDefault();
        public override void Draw(RendererBase renderer) => camInfo?.Draw(renderer);
        public override void Dispose()
        {
            camInfo = null;
        }
    }
}
