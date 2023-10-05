using Microsoft.VisualBasic;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Pixel
{
    public class RenderHost
    {
        private RendererBase m_renderer = new CRenderer();
        public RenderInfo info;
        public RendererBase GetRenderer() => m_renderer;
        public RenderHost()
        {
            info = new();
            OnRenderCompleted += info.Update;
        }
        public event Action<double>? OnRenderCompleted;
        public void SetRenderer(RendererBase renderer) => m_renderer = renderer;
        public Vector2 newResolution = default;
        public bool Rendering { get; private set; }
        private void UpdateResolution()
        {
            if (newResolution != default)
            {
                m_renderer._resolution = newResolution;
                newResolution = default;
            }
        }
        public void Render()
        {
            if (m_renderer is null)
                throw new NullReferenceException("RenderHost does not have a renderer loaded.");
            m_renderer.Dispose();

            UpdateResolution();

            if (Runtime.Current.GetStage() is Stage stage)
            {
                ShapeDrawer.Refresh(stage);
                m_renderer.Draw(stage.StageRenderInfo);
            }
            OnRenderCompleted?.Invoke(DateTime.Now.Ticks / 10000000.0);
        }
        public void MarkDirty()
        {
            m_renderer.MarkDirty();
        }
    }
}

