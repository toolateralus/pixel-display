using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace pixel_renderer
{

    public class Particle
    {
        public float maxDistFromStart = 100f;
        public float minDistFromStart = 0f;
        
        public float minTrailDist = 0.1f;
        public float maxTrailDist = 10f;
        
        public bool IsAlive = true;
        public bool Trails = false;

        Line state = new(Vector2.Zero, Vector2.One);

        public Pixel[] Pallette = new Pixel[] { Color.Purple, Color.MediumSeaGreen, Color.MediumPurple, Color.MediumBlue };
        public List<ParticleFrame> frames = new();

        public void SetLifeCycle(int duration)
        {
            Vector2 end = state.startPoint * (float)Random.Shared.NextDouble();
            for (int i = 0; i < duration; i++)
                frames.Add(new(Vector2.Zero, state.startPoint, end, Pixel.White));
        }

        int index = 0;
        public Line Next()
        {
            var frame = frames[index++];
            Vector2 newPosOffset = new Vector2(0.1f, 0.1f);
            return state = frame.Next(JRandom.Pixel(), JRandom.Vec2(Vector2.Zero, Vector2.One), 1, frame.start + newPosOffset);
        }
    }
    public class ParticleFrame
    {
        public Vector2 velocity;

        public ParticleFrame(Vector2 velocity, Vector2 start, Vector2 end, Pixel color)
        {
            this.velocity = velocity;
            this.start = start;
            this.end = end;
            this.color = color;
        }

        public Line Next(Pixel colorInput, Vector2 additionalVelocity, float lengthModifier, Vector2 newStart)
        {
            adjustColor();
            move();
            return new Line(start, end); 

            async void adjustColor()
            {
                float j = 0;
                while (j <= 1)
                {
                    color = Pixel.Lerp(color, colorInput, j);
                    j += 0.01f;
                    await Task.Delay(1);
                }
            }
            void move()
            {
                velocity += additionalVelocity;
                end.X *= lengthModifier;
                end.Y *= lengthModifier;
                start += velocity;
                end += velocity;
            }
        }

        public Vector2 start;
        public Vector2 end;
        public Pixel color;
    }

    public class Firework : Component
    {
        [Field] public Vector2 launcherPos;
        
        [Field] public int maxParticleCount = 250;
        [Field] public int initParticleCount = 100;
        
        [Field] public int maxLifeCycleLength = 1000; 
        [Field] public int minLifeCycleLength = 100; 

        [Field] public bool preWarm = true;

        Queue<Line> lines = new();
        List<Particle> state = new();

        private bool drawing;
        Random random = new();

        public override void Awake()
        {
            state = new();
            if(preWarm)
                for (int i = 0; i < initParticleCount; ++i)
                {
                    Particle p = new() 
                    {
                        // distance from launcher
                        maxDistFromStart = 300,
                        minDistFromStart = 0,

                        // distance from particle
                        maxTrailDist = 300,
                        minTrailDist = 0,

                        // other flags
                        IsAlive = false,
                        Trails = false,
                    };
                    int duration = (int)(random.NextDouble() * maxLifeCycleLength - minLifeCycleLength);
                    p.SetLifeCycle(duration);
                    state.Add(p);
                }

        }
        public override void OnDrawShapes()
        {
            drawing = true;
                for(int i = 0; lines.Count > 0; ++i)
                    ShapeDrawer.DrawLine(lines.Dequeue());

            drawing = false;
        }
        public override void Update()
        {
            if (!drawing)
                foreach (var particle in state)
                   lines.Enqueue(particle.Next());
        }
    }
}
