using Image = System.Windows.Controls.Image; 
    using System;
    using System.Collections.Generic; 
namespace pixel_renderer
{
    public enum RenderState { Game, Scene, Off , Error}
    public class RenderHost
    {
        private RendererBase? m_renderer = new CRenderer();
        public RenderInfo info = new(); 
        public RenderState State = RenderState.Game;

        public RendererBase GetRenderer() => m_renderer;
        public void SetRenderer(RendererBase renderer) => m_renderer= renderer;


        public void Render(Image renderSurface)
        {
            if (m_renderer is null)
            {
                State = RenderState.Error;
                throw new NullReferenceException("Renderer was null");
            }
            switch (State)
            {
                // non erroneous states, no action neccessary
                case RenderState.Game: break;
                case RenderState.Scene: break;

                // stop or throw
                case RenderState.Off: return;
                case RenderState.Error:
                    throw new InvalidOperationException("Rendering failed");
                default:
                    throw new Exception("Invalid case passed into RenderState selection");
            }
            Cycle(renderSurface);
        }
        /// <summary>
        /// performs the rendering loop for one cycle or frame.
        /// </summary>
        /// <param name="renderSurface"></param>
        private protected void Cycle(Image renderSurface)
        {
            m_renderer.Dispose();
            m_renderer.Draw();
            m_renderer.Render(renderSurface);
        }
    }
    public class RenderInfo 
    {
        string cachedGCValue = "";
        const int framesUntilGC_Check = 120;
        private int framesSinceGC_Check = 0;
        public int framesUntilCheck = 50;
        public int frameCount;
        public static long LastFrameTime => (long)0.01f; 

        public double Framerate => Math.Floor(1 / TimeSpan.FromTicks(DateTime.Now.Ticks - LastFrameTime).TotalSeconds * frameCount);
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
            var bytes = GC.GetTotalMemory(false) + 1f;
            var megaBytes = bytes / 1048576;
            cachedGCValue = $"gc alloc : {megaBytes} MB";
        }
    }
}

