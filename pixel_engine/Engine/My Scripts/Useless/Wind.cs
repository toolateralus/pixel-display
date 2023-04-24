using System.Numerics;
using System.Text.Json.Serialization;

namespace pixel_renderer
{
    public enum Direction
    { Left, Right, Up, Down };

    public static class Orientation
    {
        public static Vector2 GetDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Left => new Vector2(-1, 0),
                Direction.Right => new Vector2(1, 0),
                Direction.Up => new Vector2(0, -1),
                Direction.Down => new Vector2(0, 1),
                _ => Vector2.Zero,
            };
        }
    }

    internal class Wind : Component
    {
        [JsonInclude] public Direction direction = Direction.Up;
        private Rigidbody rb;
        public override void Awake() => rb = node.GetComponent<Rigidbody>();
        public override void FixedUpdate(float delta)
        {
            if (rb is null) return;
            var windDir = Orientation.GetDirection(direction);
            float speed = WaveForms.Next.Length() * 5f;
            rb.velocity += windDir * speed;
        }

        public override void Dispose()
        {
        }

        public Wind()
        { }
        public Wind(Direction direction) => this.direction = direction;
    }
}