
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
                case Direction.Up: return new Vec2(0,-1);
                case Direction.Down: return new Vec2(0,1);
                default: return Vec2.one;
            };
        }
    }
    internal class Wind : Component
    {
        Rigidbody rb;
        public Direction direction = Direction.Up; 
        public override void Awake()
        {
            rb = parentNode.GetComponent<Rigidbody>();
        }
        public override void FixedUpdate()
        {
            var windDir = Orientation.GetDirection(direction);
            float speed = -1 + WaveForms.Next.Length * 5;
            rb.velocity +=  windDir * speed;
           
        }
    } 
}
