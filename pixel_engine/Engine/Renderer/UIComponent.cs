using Newtonsoft.Json;
using System.Drawing;

namespace pixel_renderer
{
    public abstract class UIComponent : Component
    {
        [JsonProperty] public float drawOrder = 0f;
        [JsonProperty] public Vec2 bottomRightCornerOffset = new(1, 1);
        public Vec2 Center { get => parent.position; set => parent.position = value; }
        public Vec2 Size
        {
            get => bottomRightCornerOffset * 2;
            set => bottomRightCornerOffset = value * 0.5f;
        }
        abstract public void Draw(Bitmap bmp);
    }
}