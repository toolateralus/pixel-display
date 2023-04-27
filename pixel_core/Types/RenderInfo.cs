using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel
{
    public class RenderInfo
    {
        string cachedGCValue = "";
        const int framesUntilGC_Check = 120;
        private int framesSinceGC_Check = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        internal double thisFrameTime = 0;

        public RenderInfo()
        {
        }
        public double FrameTime;
        const double tenMillion = 10000000.0;
        public double Framerate => Math.Floor(1f / FrameTime);
        internal double LastFrameTime = 0;
        public double lowestFrameRate;
        public double highestFrameRate;
        public double averageFrameRate;
        Stack<double> recent = new();
        public void Update(double value)
        {
            LastFrameTime = thisFrameTime;
            thisFrameTime = value;

            if (frameCount > 0)
                FrameTime = thisFrameTime - LastFrameTime;

            if (recent.Count >= 60)
            {
                lowestFrameRate = recent.Min();
                highestFrameRate = recent.Max();
                averageFrameRate = recent.Average();
                recent.Clear();
            }
            else recent.Push(Framerate);
            frameCount++;
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

