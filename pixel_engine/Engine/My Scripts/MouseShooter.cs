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
                 
        const int initAmmoCt = 300;
        [Field] int ammoCt = 300, magazineSize = 16, currentMag = 16;

        [Field] private int speed = 70;
        [Field] private int maxParticles = 25;

        [Field] private float minVelLength = 0.001f;
        [Field] private bool particlesDieFromLowVelocity = true;
        [Field] private bool usingAmmo = false;

        private Queue<Particle> particles = new();

        void Particle(Particle p)
        {
            p.position += p.velocity;
            p.velocity *= 0.99f;
            p.size = Vector2.One;
            p.color = Color.Red;

            if(p.velocity.Length() < minVelLength && particlesDieFromLowVelocity)
                p.onDeath?.Invoke(p);

        }
        public override void Awake()
        {
            RegisterAction(Fire, Key.G);
            RegisterAction(Reload, Key.R);
            
            if(TryGetComponent<Sprite>(out var sprite)) 
                RemoveComponent(sprite);
            
            ammoCt = initAmmoCt;
            currentMag = magazineSize;
        }

        private void Fire()
        {
            Vector2 vel = (CMouse.GlobalPosition - Position) / speed;
            
            if (usingAmmo)
            {
                currentMag--;
                if (currentMag <= 0)
                {
                    Runtime.Log("You must reload.");
                    return;
                }
            }

            if (particles.Count >= maxParticles)
            {
                if (!particlesDieFromLowVelocity)
                {
                    var p = particles.Dequeue();
                    p.onDeath?.Invoke(p);
                    ReviveParticle(p, true, initPos : Position, initVel : vel, initColor: Pixel.Random);
                }
                else
                {
                    Rent(true, initPos: Position, initVel: vel, initColor: Pixel.Random);
                }
                return;
            }
            InstantiateParticle(vel);
        }

        private void InstantiateParticle(Vector2 vel)
        {
            Particle particle = new(Pixel.Random, vel, Position, Vector2.One, Particle, OnParticleDeath);
            particles.Enqueue(particle);
        }

        private void Rent(bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Pixel? initColor = null)
        {
            var p = particles.Where(p => p.dead).FirstOrDefault();

            if (p is null || p == default)
                return;

            ReviveParticle(p, reset, lifetime, death, initVel, initPos, initSize, initColor);
        }

        private static void ReviveParticle(Particle p, bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Pixel? initColor = null)
        {
            if (reset)
            {
                if (lifetime != null)
                    p.lifetime = lifetime;
                if (death != null)
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
            p.dead = true; 
        }

        private void Reload()
        {
            if (!usingAmmo)
                return;

            if (ammoCt <= 0)
            {
                Runtime.Log("Ammo Replenished! (+500)");
                ammoCt += 500; 
            }

            ammoCt -= currentMag - magazineSize;
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


                    DrawCircle(particle.position, particle.size.X, particle.color * particle.velocity.LengthSquared());
                }
            }
        }
        public override void Update()
        {

        }
    }
}
