using Image = System.Windows.Controls.Image; 
    using System;
    using System.Collections.Generic; 
namespace pixel_renderer
{
    public enum RenderState { Game, Scene, Off , Error}
    public class RenderHost
    {
        private RendererBase? m_renderer = new CRenderer();
        public RenderInfo RenderInfo = new(); 
        public RenderState State = RenderState.Game;

        public RendererBase GetRenderer() => m_renderer;
        public void SetRenderer(RendererBase renderer) => m_renderer= renderer;
        public void Render(Image renderSurface, Runtime? runtime = null)
        {
            if (runtime is null)
                if (Runtime.Instance != null) runtime = Runtime.Instance;
                else throw new ArgumentException("Runtime_Instance not found. " +
                    "This is very odd and it's likely your code base is corrupt or partially missing," +
                    " Clean and rebuild the solution.");

            if (m_renderer is null)
            {
                State = RenderState.Error;
                runtime.IsRunning = false;
                return;
            }
            switch (State)
            {
                case RenderState.Game:

                    break;
                case RenderState.Scene:

                    break;
                case RenderState.Off:

                    return;

                case RenderState.Error:
                    // replace this with some editor raising error if it's open or if it's in game view just kill the app or something.
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
        private void Cycle(Image renderSurface)
        {
            m_renderer.Dispose();
            m_renderer.Draw();
            m_renderer.Render(renderSurface);
        }
    }
    public class RenderInfo 
    {
         string cachedGCValue = "";
        const int framesUntilGC_Check = 600;
        private int framesSinceGC_Check = 0;
        public long lastFrameTime => (long)0.001f; 
        public int framesUntilCheck = 50;
        public int frameCount;
        public double Framerate => Math.Floor(1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds * frameCount);
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

