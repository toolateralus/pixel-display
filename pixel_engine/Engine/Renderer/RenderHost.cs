﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image; 

namespace pixel_renderer
{
    public enum RenderState { Game, Scene, Off , Error}
    public class RenderHost
    {
        private RendererBase m_renderer = new CRenderer();
        public RenderState State = RenderState.Game;
        public RenderInfo info;  
        public RendererBase GetRenderer() => m_renderer;
        DispatcherTimer timer = new();
        public RenderHost()
        {
            info = new(this);
            timer.Interval = TimeSpan.FromTicks(1000);
            timer.Start();
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Render();
        }

        public event Action<long>? OnRenderCompleted; 

        public void SetRenderer(RendererBase renderer) => m_renderer= renderer;

        public void Render()
        {
            if (m_renderer is null) throw new NullReferenceException("RenderHost does not have a renderer loaded."); 
            switch (State)
            {
                case RenderState.Game: break;
                case RenderState.Scene: break;
                case RenderState.Off: return;
                default:
                    throw new InvalidOperationException("Invalid case passed into RenderState selection");
            }
            Cycle();
            OnRenderCompleted?.Invoke(DateTime.Now.Ticks);
        }
        /// <summary>
        /// performs the rendering loop for one cycle or frame.
        /// </summary>
        /// <param name="renderSurface"></param>
        private protected void Cycle()
        {
            m_renderer.Dispose();
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

