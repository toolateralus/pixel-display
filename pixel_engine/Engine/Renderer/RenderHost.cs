using Image = System.Windows.Controls.Image; 
    using System;
    using System.Collections.Generic; 
namespace pixel_renderer
{

    public enum RenderState { Game, Scene, Off , Error}
    public class RenderHost
    {
        public RenderState State = RenderState.Game;
        private Runtime runtime => Runtime.Instance;

        private RendererBase? m_renderer = new CRenderer();
        public RendererBase GetRenderer() => m_renderer; 
        public void SetRenderer(RendererBase renderer) => m_renderer= renderer;
        public void Render(Image output)
        {
            if (runtime.stage is null || m_renderer is null)
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
                    throw new InvalidOperationException("Rendering failed");
                default:
                    throw new Exception("Invalid case passed into RenderState selection");
            }
            Run(output);
        }
        private void Run(Image output)
        {
            m_renderer.Dispose();
            m_renderer.Draw();
            m_renderer.Render(output);
        }
    }
    public class RenderInfo 
    {
        static string cachedGCValue = "";
        const int framesUntilGC_Check = 600;
        private static int framesSinceGC_Check = 0;
        public static string GetTotalMemory()
        {
            if (framesSinceGC_Check < framesUntilGC_Check)
            {
                framesSinceGC_Check++;
                return cachedGCValue;
            }
            framesSinceGC_Check = 0;
            update_gc_info();

            return cachedGCValue;
        }

        private static void update_gc_info()
        {
            var bytes = GC.GetTotalMemory(false) + 0.1f;
            var megaBytes = bytes / 1048576;
            cachedGCValue = $"gc alloc : {megaBytes} MB";
        }
        public static double FrameRate()
        {
            Runtime env = Runtime.Instance;
            var lastFrameTime = env.lastFrameTime;
            var frameCount = env.frameCount;
            var frameRate = Math.Floor(1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds * frameCount);

            return frameRate;
        }
    }
}

