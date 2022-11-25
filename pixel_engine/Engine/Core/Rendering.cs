namespace pixel_renderer
{
    using Image = System.Windows.Controls.Image; 
    using System;

    public enum RenderState { Game, Scene, Off , Error}
    public class Rendering
    {
        public static RenderState State = RenderState.Game;
        private static Runtime runtime => Runtime.Instance;
        private static RendererBase? m_renderer = new CRenderer();
        public static RendererBase GetRenderer() => m_renderer; 
        public static void SetRenderer(RendererBase renderer) => m_renderer= renderer;
        public static void Render( Image output)
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
            m_renderer.Dispose();
            m_renderer.Draw();
            m_renderer.Render(output);
        }
       
    }
    public class PixelGC
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

            var bytes = GC.GetTotalMemory(false) + 1;
            var megaBytes = bytes / 1048576;
            cachedGCValue = $"GC Alloc:{megaBytes} MB";

            return cachedGCValue;
        }
    }
    public class RenderStats
    {
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

