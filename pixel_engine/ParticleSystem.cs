using pixel_core.types.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace pixel_core
{
    public class Particle : Node
    {
        public float birth = 0;
        public Pixel color;
        public Vector2 Velocity
        {
            get
            {
                if (rb != null)
                    return rb.velocity;
                return Vector2.Zero;
            }
            set
            {
                if(rb != null)
                    rb.velocity = value;
            }
        }
        public Action<Particle> lifetime;
        public Action<Particle> onDeath; 
        internal bool dead;

        public Particle(Pixel initColor,
                        Vector2 initVel,
                        Vector2 initPos,
                        Vector2 initSize,
                        Action<Particle> lifetime,
                        Action<Particle> onDeath)
        {
            this.color = initColor;
            this.Velocity = initVel;
            this.Position = initPos;
            this.Scale = initSize;
            this.lifetime = lifetime;
            this.onDeath = onDeath;
        }

        public void Next() => lifetime.Invoke(this);
    }
    public class ParticleSystem : Component
    {
        [Field] public List<Pixel> Pallette = new() { Color.Purple, Color.MediumSeaGreen, Color.MediumPurple, Color.MediumBlue };
        [Field] internal List<Particle> particles = new();
        [Field] internal int speed = 70;
        [Field] private int maxParticles = 250;
        [Field] internal float minVelLength = 0.001f;
        [Field] internal bool particlesDieFromLowVelocity = false;
        public override void Dispose()
        {
            foreach (var part in particles)
                part.Destroy();

            particles.Clear();
        }
        private void ReviveParticle(bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Pixel? initColor = null)
        {
            var p = particles.Where(p => p.dead).FirstOrDefault();
            if (p is null || p == default)
                return;

            ResetParticle(p, reset, lifetime, death, initVel, initPos, initSize, initColor);
        }
        private static void ResetParticle(Particle p, bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Pixel? initColor = null)
        {
            if (reset)
            {
                if (lifetime != null)
                    p.lifetime = lifetime;
                if (death != null)
                    p.onDeath = death;
                if (initVel.HasValue)
                    p.Velocity = initVel.Value;
                if (initPos.HasValue)
                    p.Position = initPos.Value;
                if (initSize.HasValue)
                    p.Scale = initSize.Value;
                if (initColor.HasValue)
                    p.color = initColor.Value;
            }

            p.dead = false;
        }
        public virtual void InstantiateParticle(Vector2 vel)
        {
            Particle particle = new(Pixel.Random, vel, Position, Vector2.One, Cycle, OnParticleDied);
            particles.Add(particle);
        }
        public void GetParticle(Vector2 vel)
        {
            if (particles.Count >= maxParticles)
                ReviveParticle(false);
            InstantiateParticle(vel);
        }
        public virtual void Cycle(Particle p)
        {
            if (p.Velocity.SqrMagnitude() < 0.1f)
            {
                OnParticleDied(p);
                return;
            }
            p.Position += p.Velocity;
            p.Velocity *= 0.99f;

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
            p.Destroy();
        }
    }
}
