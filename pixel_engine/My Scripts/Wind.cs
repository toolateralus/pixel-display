
namespace pixel_renderer
{
    public enum Dir { Left, Right, Up, Down };
    public static class Orientation
    {
        public static Vec2 GetDirection(Dir direction)
        {
            return direction switch
            {
                Dir.Left => new Vec2(-1, 0),
                Dir.Right => new Vec2(1, 0),
                Dir.Up => new Vec2(0, -1),
                Dir.Down => new Vec2(0, 1),
                _ => Vec2.zero,
            };
        }
    }
    internal class Wind : Component
    {
        public Wind() { }
        public Wind(Dir direction) => this.direction = direction;
        
        private Rigidbody rb;
        public Dir direction = Dir.Up;
        
        public override void Awake() => rb = parent.GetComponent<Rigidbody>();
        public override void FixedUpdate(float delta)
        {
            if (rb is null) return; 
            var windDir = Orientation.GetDirection(direction);
            float speed = WaveForms.Next.Length() * 5f;
            rb.velocity += windDir * speed;
        }
    }
}
