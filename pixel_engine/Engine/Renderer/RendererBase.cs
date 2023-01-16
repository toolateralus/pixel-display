namespace pixel_renderer
{
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

    public abstract class  RendererBase 
    {
        private Bitmap fallback;
        public Bitmap FallBack
        {
            get => fallback ??= new(256, 256);
        }
        public abstract void Render(Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();
    }
    }

