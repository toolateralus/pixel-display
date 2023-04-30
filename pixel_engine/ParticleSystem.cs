using Pixel.Types.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Pixel
{
    public class Particle
    {
        public float birth = 0;
        internal bool dead;
        public Color color;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Scale { get; set; }

        public Action<Particle> lifetime;
        public Action<Particle> onDeath; 

        public void Next() => lifetime?.Invoke(this);

        internal void Set(Color random, Vector2 position, Vector2 scale, Action<Particle> cycle, Action<Particle> onParticleDied)
        {
            this.color = random;
            this.Position = position;
            this.Scale = scale;
            this.lifetime = cycle;
            this.onDeath = onParticleDied;
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
            particles.Clear();
        }
        private void ReviveParticle(bool reset, Action<Particle> lifetime = null, Action<Particle> death = null, Vector2? initVel = null, Vector2? initPos = null, Vector2? initSize = null, Color? initColor = null)
        {
            var p = particles.Where(p => p.dead).FirstOrDefault();
            if (p is null || p == default)
                return;

            ResetParticle(p, reset, lifetime, death, initVel, initPos, initSize, initColor);
        }
        public override void FixedUpdate(float delta)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                Particle? part = particles[i];
                if (part is null || part.dead)
                    continue;
               // TODO: somehow fix this
            }
        }
        public override void OnDrawShapes()
        {
            // ShapeDrawer.DrawCircleFilled(part.Position, 0.0015f);
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
            Particle particle = new();
            
            particle.Set(Color.Random, Position, Vector2.One, Cycle, OnParticleDied);

            particles.Add(particle);
        }
        public void GetParticle(Vector2 vel)
        {
            if (particles.Count >= maxParticles)
                ReviveParticle(false);
             else InstantiateParticle(vel);

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
        }
        public virtual void OnParticleDied(Particle p)
        {
            if (p.dead)
                return;

            p.onDeath?.Invoke(p); 
            p.dead = true;
        }
    }
}
