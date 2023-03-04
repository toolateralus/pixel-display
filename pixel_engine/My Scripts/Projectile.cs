namespace pixel_renderer
{
    public class Projectile : Component
    {
        public float hitRadius;
        public Node? sender; 

        public override void Awake()
        {
        }

        public override void FixedUpdate(float delta)
        {
        }

        public override void OnCollision(Collider collider)
        {
        }

        public override void OnDrawShapes()
        {
            if (sender is null)
                ShapeDrawer.DrawCircle(Position + Scale / 2, 16, Pixel.Red);
              else
              {
                  ShapeDrawer.DrawLine(Position + Scale / 2, sender.Position + sender.Scale / 2, Pixel.White);
                  ShapeDrawer.DrawCircle(Position + Scale / 2, 16, Pixel.White);
              }
        }

        public override void OnTrigger(Collider other)
        {
        }
    }
}