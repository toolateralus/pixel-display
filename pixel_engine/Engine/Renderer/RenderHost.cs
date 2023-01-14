using Image = System.Windows.Controls.Image; 
    using System;
    using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace pixel_renderer
{
    public enum RenderState { Game, Scene, Off, Error }
    public class RenderHost
    {
        private CRenderer? m_renderer = new CRenderer();
        public RenderState State = RenderState.Game;
        public RenderInfo info;
        public RendererBase GetRenderer() => m_renderer;

        public RenderHost()
        {
            info = new(this);
        }

        public event Action<long> OnRenderCompleted;

        public void SetRenderer(CRenderer renderer) => m_renderer = renderer;

        public void Render(Image renderSurface)
        {
            if (m_renderer is null) 
                throw new NullReferenceException("RenderHost does not have a renderer loaded.");
            switch (State)
            {
                case RenderState.Game: break;
                case RenderState.Scene: break;
                case RenderState.Off: return;
                default:
                    throw new InvalidOperationException("Invalid case passed into RenderState selection");
            }
            m_renderer.Render(renderSurface);
            OnRenderCompleted?.Invoke(DateTime.Now.Ticks);
        }
        internal protected void Next()
        {
            m_renderer.Dispose();
            m_renderer.Draw();
        }
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
            public double Framerate => (double)Math.Floor((double)1 / ((double)FrameTime / (double)10_000_000));

            public void Update(long value)
            {
                lastFrameTime = thisFrameTime;
                thisFrameTime = value;
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
                cachedGCValue = $"gc alloc : {megaBytes} MB";
            }
            private static float BytesToMegaBytes(float bytes)
            {
                return bytes / 1_048_576;
            }
        }
    }
}
