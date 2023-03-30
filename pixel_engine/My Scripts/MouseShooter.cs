using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
        private Queue<Particle> particles = new();
        private bool particlesBusy;

        private int maxParticles = 2000;

        void Particle(Particle p)
        {
            p.position += p.velocity;
            p.velocity *= 0.99f;
            p.size = Vector2.One;
            p.color = Color.Red; 

            if (p.velocity.SqrMagnitude() < 0.001f)
                p.onDeath?.Invoke(p); 
        }
        public override void Awake()
        {
            RegisterAction(Fire, Key.G);
            RegisterAction(() => fired = false, Key.G, InputEventType.KeyUp);
            RegisterAction(Reload, Key.R);
            if(TryGetComponent<Sprite>(out var sprite)) RemoveComponent(sprite);
            
            ammoCt = initAmmoCt;
            currentMag = magazineSize;
        }

        private void Fire()
        {
            Vector2 vel = (CMouse.GlobalPosition - Position) / speed;
            fired = true;

            if (particles.Count >= maxParticles)
            {
                Runtime.Log("particle recycled.");
                Rent(true, initPos: Position, initVel: vel, initColor: Pixel.Random);
                return;
            }
            InstantiateParticle(vel);
        }

        private void InstantiateParticle(Vector2 vel)
        {
            Particle particle = new(Pixel.Random, vel, Position, Vector2.One, Particle, OnParticleDeath);

            particlesBusy = true;
            particles.Enqueue(particle);
            particlesBusy = false;
        }

        private void Rent(bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Pixel? initColor = null)
        {
            var p = particles.Where(p => p.dead).FirstOrDefault();

            if (p is null || p == default)
                p = particles.Dequeue(); 

            if (reset)
            {
                if(lifetime != null)
                    p.lifetime = lifetime;
                if(death != null)
                    p.onDeath = death;
                if (initVel.HasValue)
                    p.velocity = initVel.Value;
                if (initPos.HasValue)
                    p.position = initPos.Value;
                if (initSize.HasValue)
                    p.size = initSize.Value;
                if (initColor.HasValue)
                    p.color = initColor.Value;
            }

            p.dead = false;
        }

        private void OnParticleDeath(Particle p)
        {
            p.position = Position;
            p.dead = true; 
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
                foreach(var particle in particles)
                {
                    particle?.Next();
                    if (particle is null || particle.dead) 
                        continue;
                    DrawCircle(particle.position, particle.size.X, particle.color);
                }
            }
        }
        public override void Update()
        {

        }
    }
}
