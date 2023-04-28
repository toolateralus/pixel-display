using Pixel.Types.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Pixel
{
    public class Particle : Component
    {
        public float birth = 0;
        internal bool dead;
        public Color color;

        public Action<Particle> lifetime;
        public Action<Particle> onDeath; 

        public Particle(Color initColor, Vector2 initPos, Vector2 initSize, Action<Particle> lifetime, Action<Particle> onDeath)
        {
            this.color = initColor;
            this.Position = initPos;
            this.Scale = initSize;
            this.lifetime = lifetime;
            this.onDeath = onDeath;
        }

        public Particle()
        {
        }

        public void Next() => lifetime.Invoke(this);

        public override void Dispose()
        {
        }
    }
    public class ParticleSystem : Component
    {
        [Field] public List<Color> Pallette = new() { System.Drawing.Color.Purple, System.Drawing.Color.MediumSeaGreen, System.Drawing.Color.MediumPurple, System.Drawing.Color.MediumBlue };
        [Field] internal List<Particle> particles = new();
        [Field] internal int speed = 70;
        [Field] private int maxParticles = 250;
        [Field] internal float minVelLength = 0.001f;
        [Field] internal bool particlesDieFromLowVelocity = false;
        public override void Dispose()
        {
            foreach (var part in particles)
                part.node.Destroy();

            particles.Clear();
        }
        private void ReviveParticle(bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Color? initColor = null)
        {
            var p = particles.Where(p => p.dead).FirstOrDefault();
            if (p is null || p == default)
                return;

            ResetParticle(p, reset, lifetime, death, initVel, initPos, initSize, initColor);
        }
        private static void ResetParticle(Particle p, bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Color? initColor = null)
        {
            if (reset)
            {
                if (lifetime != null)
                    p.lifetime = lifetime;
                if (death != null)
                    p.onDeath = death;
                if (initVel.HasValue)
                    p.GetComponent<Rigidbody>().velocity = initVel.Value;
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
            Node node = new("temp_particle");

            var particle = node.AddComponent<Particle>();
            particle = new(Color.Random, Position, Vector2.One, Cycle, OnParticleDied);

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
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb.velocity.SqrMagnitude() < 0.1f)
            {
                OnParticleDied(p);
                return;
            }
            var col = JRandom.Color();
            _ = color();
            async Task color()
            {
                float j = 0;
                while (j <= 1)
                {
                    p.color = Color.Blend(p.color, col, j);
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
            p.node.Destroy();
        }
    }
}
