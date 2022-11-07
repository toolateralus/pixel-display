
namespace pixel_renderer
{
    public class Camera : Component
    {
        public override void Awake()
        {
            base.Awake();
        }

        public Vec2 rect = new()
        {
            x = 32,
            y = 32
        };
    }
}