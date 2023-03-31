using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        [Field] private List<Particle> particles = new();
        [Field] private Random random = new();
        [Field] internal bool debugBase = true;
        

        public override void Awake()
        {
            if(debugBase)
                for (int i = 0; i < 10; ++i)
                {
                    Vector2 initVel = GetRandomVelocity(0.01f);
                    Particle particle = new(initVel, Cycle);
                    particles.Add(particle);
                }
        }
        public override void OnDrawShapes()
        {
            if(debugBase)
                lock (particles)
                   foreach (var p in particles)
                   {
                        if (p.dead)
                            continue;

                        p.Next();
                        ShapeDrawer.DrawRect(p.position, p.position * p.velocity.Length(), p.color);
                   }

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
        public void OnParticleDied(Particle p)
        {
            if (p.dead)
                return;

            p.onDeath?.Invoke(p); 
            p.dead = true;

            p.position = Position;
            p.velocity = GetRandomVelocity(); 

            p.dead = false; 
        }
        private Vector2 GetRandomVelocity(float speed = -1f)
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
