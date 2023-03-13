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
        public Vector2 velocity;
        public Vector2 position;
        public Action<Particle> lifetime;

        public float lifespan;
        internal bool dead;

        public Particle(Vector2 initVel, Action<Particle> lifetime)
        {
            velocity = initVel;
            this.lifetime = lifetime;
        }

        public void Next() => lifetime.Invoke(this);
    }
    public class ParticleSystem : Component
    {
        [Field] public List<Pixel> Pallette = new() { Color.Purple, Color.MediumSeaGreen, Color.MediumPurple, Color.MediumBlue };
        [Field] private float maxParticleSpeed = 15f;
        [Field] private List<Particle> particles = new();
        [Field] private Random random = new();

        public void OnParticleDied(Particle p)
        {
            if (p.dead)
                return;
            p.dead = true;

            p.position = Position;
            p.velocity = GetRandomVelocity(500); 

            p.dead = false; 
        }
        public override void Awake()
        {
            for (int i = 0; i < 10; ++i)
            {
                float speed;
                Vector2 initVel = GetRandomVelocity();
                initVel = new(1, 1);

                Particle particle = new(initVel, particleLifetime);
                particles.Add(particle);

                void particleLifetime(Particle p)
                {
                    if (p.velocity.SqrMagnitude() < 0.5f)
                    {

                        OnParticleDied(p);
                        return;
                    }

                    p.position += p.velocity;
                    p.velocity *= 0.99f;

                    var col = Pixel.White;
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
            }
        }

        private Vector2 GetRandomVelocity(float speed = -1f)
        {
            float x = (float)random.NextDouble();
            float y = (float)random.NextDouble();
            if(speed == -1f)
             speed = (float)random.NextDouble() * maxParticleSpeed;
            return (new Vector2(x, y) * speed).Normalized();
        }

        public override void Update()
        {
           
        }
        public override void OnDrawShapes()
        {
            lock (particles)
               foreach (var p in particles)
               {
                    if (p.dead)
                        continue;

                    p.Next();
                    ShapeDrawer.DrawCircle(p.position, p.velocity.SqrMagnitude() + 2, p.color);
               }

        }


      
    }
}
