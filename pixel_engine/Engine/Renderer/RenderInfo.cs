using System;
using System.Collections.Generic;
using System.Linq;

namespace pixel_renderer
{
    public class RenderInfo 
    {
        string cachedGCValue = "";
        const int framesUntilGC_Check = 120;
        private int framesSinceGC_Check = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        internal long lastFrameTime = 0;
        internal long thisFrameTime = 0;

        public RenderInfo(RenderHost renderer)
        {
            renderer.OnRenderCompleted += Update; 
        }
        public long FrameTime => thisFrameTime - lastFrameTime;
        const double tenMillion = 10000000.0; 
        public double Framerate => Math.Floor(1 / (FrameTime / tenMillion));
        
        public double lowestFrameRate;
        public double highestFrameRate;
        public double averageFrameRate;

        Stack<double> recent = new();
        public void Update(long value)
        {
            lastFrameTime = thisFrameTime;
            thisFrameTime = value;

            if (recent.Count >= 60)
            {
                lowestFrameRate = recent.Min();
                highestFrameRate = recent.Max();
                averageFrameRate = recent.Average();
                recent.Clear();
            }
            else recent.Push(Framerate);
        }
       
        public string GetTotalMemory()
        {
            if (framesSinceGC_Check < framesUntilGC_Check)
            {
                framesSinceGC_Check++;
                return cachedGCValue;
            }
            framesSinceGC_Check = 0;
            UpdateGCInfo();
            return cachedGCValue;
        }
        private void UpdateGCInfo()
        {
            var bytes = GC.GetTotalMemory(true) + 1f;
            float megaBytes = BytesToMegaBytes(bytes);
            cachedGCValue = $"GC: {megaBytes}Mb";
        }
        private static float BytesToMegaBytes(float bytes)
        {
            return bytes / 1_048_576;
        }
    }
}

