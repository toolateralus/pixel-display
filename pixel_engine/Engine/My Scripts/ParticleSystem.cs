using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class Particle 
    {
        public Pixel color;
        public float birth = 0;
        public Vector2 velocity;
        public Vector2 position;
        public Vector2 size;
        public Action<Particle> lifetime;
        public Action<Particle> onDeath; 
        internal bool dead;

        public Particle(Vector2 initVel, Action<Particle> lifetime)
        {
            velocity = initVel;
            this.lifetime = lifetime;
        }
        public Particle(Action<Particle> Lifetime, Action<Particle> onDeath)
        {
            lifetime = Lifetime;
            this.onDeath = onDeath;
        }
        public Particle(Pixel initColor, Vector2 initVel, Vector2 initPos, Vector2 initSize, Action<Particle> lifetime, Action<Particle> onDeath)
        {
            this.color = initColor;
            this.velocity = initVel;
            this.position = initPos;
            this.size = initSize;
            this.lifetime = lifetime;
            this.onDeath = onDeath;
        }

        public void Next() => lifetime.Invoke(this);
    }
    public class ParticleSystem : Component
    {
        [Field] public List<Pixel> Pallette = new() { Color.Purple, Color.MediumSeaGreen, Color.MediumPurple, Color.MediumBlue };
        [Field] private float maxParticleSpeed = 15f;
        [Field] internal Queue<Particle> particles = new();
        [Field] private Random random = new();
        [Field] internal int speed = 70;
        [Field] private int maxParticles = 250;

        [Field] internal float minVelLength = 0.001f;
        [Field] internal bool particlesDieFromLowVelocity = false;

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

        private void InstantiateParticle(Vector2 vel)
        {
            Particle particle = new(Pixel.Random, vel, Position, Vector2.One, Cycle, OnParticleDied);
            particles.Enqueue(particle);
        }

        public void GetParticle(Vector2 vel)
        {

            if (particles.Count >= maxParticles)
            {
                if (!particlesDieFromLowVelocity)
                {
                    var p = particles.Dequeue();
                    p.onDeath?.Invoke(p);
                    ReviveParticle(p, true, initPos: Position, initVel: vel, initColor: Pixel.Random);
                }
                else
                {
                    Rent(true, initPos: Position, initVel: vel, initColor: Pixel.Random);
                }
                return;
            }
            InstantiateParticle(vel);
        }
        
        public virtual void Cycle(Particle p)
        {
            if (p.velocity.SqrMagnitude() < 0.1f)
            {
                OnParticleDied(p);
                return;
            }
            p.position += p.velocity;
            p.velocity *= 0.99f;

            var col = JRandom.Color();
            _ = color();

            async Task color()
            {
                float j = 0;
                while (j <= 1)
                {
                    p.color = Pixel.Lerp(p.color, col, j);
                    j += 0.01f;
                    await Task.Delay(1);
                }
                return;
            }

        }
        public virtual void OnParticleDied(Particle p)
        {
            if (p.dead)
                return;

            p.onDeath?.Invoke(p); 
            p.dead = true;
        }
        public Vector2 GetRandomVelocity(float speed = -1f)
        {
            float x = (float)random.NextDouble() * 2 - 1;
            float y = (float)random.NextDouble() * 2 - 1;
            var dir = new Vector2(x, y).Normalized();
            if (speed == -1f)
             speed = (float)random.NextDouble() * maxParticleSpeed;
            return (dir * speed);
        }
    }
}
