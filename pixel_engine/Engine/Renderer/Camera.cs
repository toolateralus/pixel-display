using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace pixel_renderer
{
    public class Camera : UIComponent
    {
        [JsonProperty] public Vec2 viewportPosition = Vec2.zero;
        [JsonProperty] public Vec2 viewportSize = Vec2.one;
        [JsonProperty] public float angle = 0f;
        [JsonProperty] public DrawingType DrawMode = DrawingType.Wrapped;
        public float[,] zBuffer = new float[0, 0];

        public Vec2 GlobalToViewport(Vec2 global)
        {
            Vec2 relativePosition = (global - Center).Rotated(angle) + bottomRightCornerOffset;
            return relativePosition / Size.GetDivideSafe() * viewportSize + viewportPosition;
        }

        public Vec2 ViewportToGlobal(Vec2 vpPos)
        {
            Vec2 relativePosition = (vpPos - viewportPosition) / viewportSize.GetDivideSafe() * Size;
            return (relativePosition - bottomRightCornerOffset).Rotated(-angle) + Center;
        }
    }
    public enum DrawingType { Wrapped, Clamped, None }
}
