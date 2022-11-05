
namespace pixel_renderer
{
    public class Camera : Component
    {
        public override string UUID { get; set; }
        public override string Name { get; set; }
        public Vec2 rect = new()
        {
            x = 32,
            y = 32
        };
    }
}