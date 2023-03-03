using Newtonsoft.Json;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    public class Camera : UIComponent
    {
        [Field] [JsonProperty] public Vector2 viewportPosition = Vector2.Zero;
        [Field] [JsonProperty] public Vector2 viewportSize = Vector2.One;
        [Field] [JsonProperty] public DrawingType DrawMode = DrawingType.Clamped;
        public float[,] zBuffer = new float[0, 0];

        public Vector2 LocalToCamViewport(Vector2 local) =>
            local / Size.GetDivideSafe();

        public Vector2 CamViewportToLocal(Vector2 camViewport) => camViewport * Size;
        public Vector2 CamToScreenViewport(Vector2 camViewport) => camViewport * viewportSize + viewportPosition;
        public Vector2 ScreenToCamViewport(Vector2 screenViewport)
        {
            viewportSize.MakeDivideSafe();
            return (screenViewport - viewportPosition) / viewportSize;
}

        public Vector2 GlobalToCamViewport(Vector2 global) => LocalToCamViewport(GlobalToLocal(global));
        public Vector2 ViewportToGlobal(Vector2 camViewport) => LocalToGlobal(CamViewportToLocal(camViewport));
        public Vector2 GlobalToScreenViewport(Vector2 global) => CamToScreenViewport(GlobalToCamViewport(global));
        public Vector2 ScreenViewportToLocal(Vector2 screenViewport) => CamViewportToLocal(ScreenToCamViewport(screenViewport));
        public Vector2 ScreenViewportToGlobal(Vector2 screenViewport) => LocalToGlobal(ScreenViewportToLocal(screenViewport));
        public Vector2 ViewportToSpriteViewport(Sprite sprite, Vector2 viewportPos) =>
            sprite.GlobalToViewport(ViewportToGlobal(viewportPos));
        public Vector2 ViewportToSpriteViewport(SpriteInfo sprite, Vector2 viewportPos) =>
            sprite.GlobalToViewport(ViewportToGlobal(viewportPos));
        public static Camera? First => Runtime.Current.GetStage()?.GetAllComponents<Camera>().First();
    }
    public enum DrawingType { Wrapped, Clamped, None }
}
