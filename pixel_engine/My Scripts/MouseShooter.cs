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
        [Field]
        public float aimDistance = 3f;
        private int speed = 60;
        private Particle particle;

        public override void Awake()
        {
            RegisterAction(Shoot, Key.LeftCtrl);
            
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
            fired = true;
            Vector2 vel = (CMouse.GlobalPosition - Position) / speed;
            particle = new Particle(vel, Particle);
        }
        void Particle(Particle p)
        {
            p.position += p.velocity;
            p.velocity *= 0.99f;
            
            var l = p.velocity.Length();
            
            p.size = new(l, l);

            if (p.velocity.SqrMagnitude() < 0.01f)
            {
                particle = null;
                fired = false; 
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
            particle?.Next();

            if (particle is null) 
                return;

            DrawCircle(particle.position, particle.size.X, particle.color);
        }
        public override void Update()
        {

        }
    }
}
