using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image; 

namespace pixel_renderer
{
    public class RenderHost
    {
        private RendererBase m_renderer = new CRenderer();
        public RenderInfo info;  
        public RendererBase GetRenderer() => m_renderer;
        
        DispatcherTimer timer = new();

        public RenderHost()
        {
            info = new(this);
            timer.Tick += Timer_Tick;
            timer.Interval = new(1);
            timer.Start();
        }

        private void Timer_Tick(object? o, EventArgs e)
        {
            if (Runtime.Instance.IsRunning) 
                Render();
        }

        public event Action<long>? OnRenderCompleted; 

        public void SetRenderer(RendererBase renderer) => m_renderer= renderer;

        public Vec2? newResolution = null;

        public bool Rendering { get; private set; }

        private void UpdateResolution()
        {
            if (newResolution != null)
            {
                m_renderer.resolution = (Vec2)newResolution;
                newResolution = null;
            }
        }

        public void Render()
        {
            if (m_renderer is null) throw new NullReferenceException("RenderHost does not have a renderer loaded.");
            try
            {

                Cycle();
            }
            catch
            (Exception e)
            {
                Runtime.Log(e.Message);
            }

            OnRenderCompleted?.Invoke(DateTime.Now.Ticks);
        }
        /// <summary>
        /// performs the rendering loop for one cycle or frame.
        /// </summary>
        /// <param name="renderSurface"></param>
        private protected void Cycle()
        {
            m_renderer.Dispose();
            UpdateResolution();
            if (Runtime.Instance.GetStage() is Stage stage)
            {
                ShapeDrawer.Refresh(stage);
                m_renderer.Draw(stage.StageRenderInfo);
            }
        }

        public void MarkDirty()
        {
            m_renderer.MarkDirty();
        }
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

