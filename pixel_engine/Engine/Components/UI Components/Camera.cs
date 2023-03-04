using Newtonsoft.Json;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    public class Camera : UIComponent
    {
        [Field] [JsonProperty] public Vector2 viewportPosition = Vector2.Zero;
        [Field] [JsonProperty] public Vector2 viewportSize = Vector2.One;
        public float[,] zBuffer = new float[0, 0];
        public Vector2 LocalToScreenViewport(Vector2 local) => local * viewportSize + viewportPosition;
        public Vector2 ScreenViewportToLocal(Vector2 screenViewport)
        {
            viewportSize.MakeDivideSafe();
            return (screenViewport - viewportPosition) / viewportSize;
        }
        public Vector2 GlobalToScreenViewport(Vector2 global) => LocalToScreenViewport(GlobalToLocal(global));
        public Vector2 ScreenViewportToGlobal(Vector2 screenViewport) => LocalToGlobal(ScreenViewportToLocal(screenViewport));
        public Vector2 LocalToSpriteViewport(Sprite sprite, Vector2 local) =>
            sprite.GlobalToViewport(LocalToGlobal(local));
        public Vector2 LocalToSpriteLocal(SpriteInfo sprite, Vector2 local) =>
            sprite.GlobalToLocal(LocalToGlobal(local));
        public static Camera? First => Runtime.Current.GetStage()?.GetAllComponents<Camera>().First();
    }
    public enum DrawingType { Wrapped, Clamped, None }
}
