using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace pixel_renderer
{
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
