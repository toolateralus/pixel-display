using Newtonsoft.Json;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
    public abstract class UIComponent : Component
    {
        [Field][JsonProperty] public float drawOrder = 0f;
        [Field][JsonProperty] public float angle = 0f;
        [Field][JsonProperty] public Vector2 bottomRightCornerOffset = new(1, 1);

        public Vector2 Center { get => parent.Position; set => parent.Position = value; }
        public Vector2 Size
        {
            get => bottomRightCornerOffset * 2;
            set => bottomRightCornerOffset = value * 0.5f;
        }
        
        public Vector2 GlobalToLocal(Vector2 global) => (global - Center).Rotated(angle) + bottomRightCornerOffset;
        public Vector2 LocalToGlobal(Vector2 local) => (local - bottomRightCornerOffset).Rotated(-angle) + Center;
    }
}