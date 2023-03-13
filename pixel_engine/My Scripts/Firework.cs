using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Data;
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

    public class LineAnimation
    {
        public bool IsAlive = true;
        public List<LineFrame> frames = new();

        int index = 0;
        private Vector2 origin;
        private Vector2 extents;
        public List<Pixel> Pallette = new() { Color.Purple, Color.MediumSeaGreen, Color.MediumPurple, Color.MediumBlue };

        public void SetLifeCycle(Vector2 origin, Vector2 extents, Curve velocity, List<Pixel> colors)
        {
            if (velocity.points.Count < colors.Count)
                return;

            this.origin = origin;
            this.extents = extents;
            var zero = Vector2.Zero;
            foreach (var color in colors)
            {
                var pos = velocity.Next();
                var end = pos + velocity.Next();
                frames.Add(new(zero, pos, end, color));
            }
        }

        Vector2 accStart;
        Vector2 accEnd;

        public Line? Next()
        {
            if (frames.Count == 0)
                return null; 

            if (frames.Count <= index)
                index = 0;

            var frame = frames[index];
            index++;

            Pixel colorToLerpTo = JRandom.Pixel();
            Vector2 newPosOffset = new Vector2(0.003f, -0.001f);
            Vector2 newEndOffset = new Vector2(0.001f, -0.001f);
            Vector2 additionalVel = new Vector2(0.001f, -0.001f);


            if (accStart.IsWithin(origin, extents))
                accStart += newPosOffset;
            else accStart = origin;

            if (accEnd.IsWithin(origin, extents))
                accEnd += newEndOffset;
            else accEnd = origin;
            

            return frame.Next(colorToLerpTo, additionalVel,  accEnd, accStart);
        }

        int colorIndex= 0;

        internal Pixel NextColor()
        {
            if (Pallette.Count == 0) return Pixel.Black;
            if (colorIndex >= Pallette.Count)
                colorIndex = 0;
            return Pallette[colorIndex++];
        }

    }
    public class LineFrame
    {
        public Vector2 velocity;

        public LineFrame(Vector2 velocity, Vector2 start, Vector2 end, Pixel color)
        {
            this.velocity = velocity;
            this.start = start;
            this.end = end;
            this.color = color;
        }

        public Line Next(Pixel colorInput, Vector2 additionalVelocity, Vector2 newEnd, Vector2 newStart)
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
                start = newStart;
                end = newEnd;
                velocity += additionalVelocity;

                if (velocity != Vector2.Zero)
                {
                    start *= velocity;
                    end *= velocity;
                }
               
            }
        }

        public Vector2 start;
        public Vector2 end;
        public Pixel color;
    }
    public class LineAnimator : Component
    {
        [Field] public Vector2 launcherPos;
        
        [Field] public int maxParticleCount = 250;
        [Field] public int initParticleCount = 10;
        [Field] public int maxDist = 1000; 
        [Field] public bool preWarm = true;

        Queue<Line> lines = new();
        List<LineAnimation> state = new();

        private bool drawing;
        Random random = new();

        public override void Awake()
        {
            drawing = true;
            state = new();
            if(preWarm)
                for (int i = 0; i < initParticleCount; ++i)
                {
                    LineAnimation p = new();
                    int duration = (int)(random.NextDouble() * maxDist);
                    p.SetLifeCycle(launcherPos, launcherPos * maxDist, Curve.Circlular(1, 16, 16, true), p.Pallette);
                    state.Add(p);
                }

            drawing = false; 

        }
        public override void OnDrawShapes()
        {
            drawing = true;
            for (int i = 0; lines.Count > 0; ++i)
            {
                var line = lines.Dequeue();
                ShapeDrawer.DrawLine(line.startPoint, line.endPoint, state[i].NextColor());
            }
            drawing = false;
        }
        public override void Update()
        {
            if (!drawing)
                foreach (var particle in state)
                {
                    var next = particle.Next();
                    if (next is not null && lines.Count < maxParticleCount)
                            lines.Enqueue(next);
                } 

        }
    }
}
