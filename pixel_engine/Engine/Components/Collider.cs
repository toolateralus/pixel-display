

namespace pixel_renderer
{
    public class Collider : Component
    {
        public Vec2 size = new(0,0);
        public Sprite? spr;

        public bool IsTrigger { get; internal set; }
        public override void Awake()
        { 

        }
        public override void FixedUpdate(float delta)
        {
        }
        public override void OnCollision(Collider collider)
        {
        }
        public override void OnTrigger(Collider other)
        {
        }
        public override void Update()
        {
        }
    }
}
