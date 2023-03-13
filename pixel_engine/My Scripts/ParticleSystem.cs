using pixel_renderer.ShapeDrawing;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;


namespace pixel_renderer
{
    public record Particle
    {
        public Particle(Vector2 velocity, Pixel color)
        {
            this.start += velocity;
            this.end += velocity;
            this.color = color;
        }
        public Vector2 start;
        public Vector2 end;
        public Pixel color;
    }
    public class ParticleSystem : Component
    {
        [Field] public List<Pixel> Pallette = new() { Color.Purple, Color.MediumSeaGreen, Color.MediumPurple, Color.MediumBlue };
        [Field] public Vector2 launcherPos;
        [Field] public int maxParticleCount = 1;
        [Field] public int initParticleCount = 10;
        [Field] public int maxDist = 250; 
        [Field] public bool preWarm = true;
        [Field] private List<Particle> state = new();

        int index = 0;
        public override void Awake()
        {
            state = new();
            if(preWarm)
                for (int i = 0; i < initParticleCount; ++i)
                {
                    Particle p = new(Position);
                    state.Add(p);
                }

        }
        public override void Update()
        {
            if (index >= state.Count)
                index = 0;
            Particle particle = state[index++];
        }
        public override void OnDrawShapes()
        {
           
        }


      
    }
}
