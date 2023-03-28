using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using static pixel_renderer.Input;
using static pixel_renderer.ShapeDrawing.ShapeDrawer;
namespace pixel_renderer
{
    public class MouseShooter : Component
    {
        private bool fired;

        [Field]
        public Key fireKey = Key.F,
                  reloadKey = Key.R;
        [Field]
        public int ammoCt = 300,
                   magazineSize = 16,
                   currentMag = 16;

        const int initAmmoCt = 300;
        private int speed = 60;
        private Particle particle;

        void Particle(Particle p)
        {
            p.position += p.velocity;
            p.velocity *= 0.99f;
            
            var l = p.velocity.Length();
            
            p.size = new(l, l);

            if (p.velocity.SqrMagnitude() < 0.01f)
                particle.onDeath?.Invoke(); 
        }
        public override void Awake()
        {
            RegisterAction(Shoot, Key.G);
            
            ammoCt = initAmmoCt;
            currentMag = magazineSize;
        }
        private void Shoot()
        {
            Fire();

            if (Get(Key.R))
                Reload();
        }
        private void Fire()
        {
            if (particle is null)
            {
                Vector2 vel = (CMouse.GlobalPosition - Position) / speed;
                fired = true;
                particle = new Particle(vel, Particle);
                particle.onDeath += delegate { fired = false; }; 
            }
        }
        private void Reload()
        {
            ammoCt -= magazineSize;
            currentMag = magazineSize;
        }
        public override void FixedUpdate(float delta)
        {
            Position = Camera.First.Position;
        }
        public override void OnDrawShapes()
        {
            if (Runtime.IsRunning)
            {
                particle?.Next();
                if (particle is null) 
                    return;
                DrawCircle(particle.position, particle.size.X, particle.color);
            }
        }
        public override void Update()
        {

        }
    }
}
