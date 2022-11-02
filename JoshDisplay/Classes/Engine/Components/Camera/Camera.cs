using pixel_renderer.Components;
using PixelRenderer;
using PixelRenderer.Components;

namespace pixel_renderer.Components
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