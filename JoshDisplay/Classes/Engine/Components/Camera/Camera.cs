using PixelRenderer;
using PixelRenderer.Components;

namespace PixelRenderer.Components
{
    public class Camera : Component
    {
        public Vec2 rect = new()
        { 
            x = 32,
            y = 32 
        };
    }
}