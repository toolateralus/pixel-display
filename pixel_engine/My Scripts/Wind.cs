
namespace pixel_renderer
{
    public enum Direction { Left, Right, Up, Down };
    public static class Orientation
    {
        public static Vec2 GetDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left: return new Vec2(-1, 0);
                case Direction.Right: return new Vec2(1, 0);
                case Direction.Up: return new Vec2(0, -1);
                case Direction.Down: return new Vec2(0, 1);
                default: return Vec2.one;
            };
        }
    }
    internal class Wind : Component
    {
        Rigidbody rb;
        public Direction direction = Direction.Up;
        public Wind() { }
        public Wind(Direction direction) => this.direction = direction;
        public override void Awake() => rb = parentNode.GetComponent<Rigidbody>();
        public override void FixedUpdate(float delta)
        {
            if (rb is null) return; 
            var windDir = Orientation.GetDirection(direction);
            float speed = WaveForms.Next.Length() * 5f;
            rb.velocity += windDir * speed;
        }
    }
}
