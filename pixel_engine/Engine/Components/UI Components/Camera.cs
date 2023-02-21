using Newtonsoft.Json;
using System.Linq;

namespace pixel_renderer
{
    public class Camera : UIComponent
    {
        [Field] [JsonProperty] public Vec2 viewportPosition = Vec2.zero;
        [Field] [JsonProperty] public Vec2 viewportSize = Vec2.one;
        [Field] [JsonProperty] public DrawingType DrawMode = DrawingType.Clamped;
        public float[,] zBuffer = new float[0, 0];

        public Vec2 LocalToCamViewport(Vec2 local) => local / Size.GetDivideSafe();
        public Vec2 CamViewportToLocal(Vec2 camViewport) => camViewport * Size;
        public Vec2 CamToScreenViewport(Vec2 camViewport) => camViewport * viewportSize + viewportPosition;
        public Vec2 ScreenToCamViewport(Vec2 screenViewport) => (screenViewport - viewportPosition) / viewportSize.GetDivideSafe();
        public Vec2 GlobalToCamViewport(Vec2 global) => LocalToCamViewport(GlobalToLocal(global));
        public Vec2 ViewportToGlobal(Vec2 camViewport) => LocalToGlobal(CamViewportToLocal(camViewport));
        public Vec2 GlobalToScreenViewport(Vec2 global) => CamToScreenViewport(GlobalToCamViewport(global));
        public Vec2 ScreenViewportToLocal(Vec2 screenViewport) => CamViewportToLocal(ScreenToCamViewport(screenViewport));
        public Vec2 ScreenViewportToGlobal(Vec2 screenViewport) => LocalToGlobal(ScreenViewportToLocal(screenViewport));

        public Vec2 ViewportToSpriteViewport(Sprite sprite, Vec2 viewportPos) =>
            sprite.GlobalToViewport(ViewportToGlobal(viewportPos));
        public static Camera? First => Runtime.Current.GetStage()?.GetAllComponents<Camera>().First();
    }
    public enum DrawingType { Wrapped, Clamped, None }
}
