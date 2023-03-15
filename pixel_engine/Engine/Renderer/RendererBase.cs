namespace pixel_renderer
{
    using pixel_renderer.ShapeDrawing;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public abstract class  RendererBase 
    {
        Vector2 zero = Vector2.Zero;
        Vector2 one = Vector2.One;
        internal protected JImage baseImage;
        private protected byte[] frame = Array.Empty<byte>();
        private protected byte[] latestFrame = Array.Empty<byte>();
        private protected int stride = 0;
        public byte[] Frame => latestFrame;
        public int Stride => stride;
        public Vector2 Resolution 
        { 
            get => _resolution;
            set => Runtime.Current.renderHost.newResolution = value; 
        }
        internal protected Vector2 _resolution = Constants.DefaultResolution;
        public bool baseImageDirty = true;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public abstract void Render(System.Windows.Controls.Image output);
        public abstract void Draw(StageRenderInfo info);
        public abstract void Dispose();
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void WriteColorToFrame(ref Pixel color, ref Vector2 framePos)
        {
            int index = (int)framePos.Y * stride + ((int)framePos.X * 3);

            float colorB = (float)color.b / 255 * color.a;
            float colorG = (float)color.g / 255 * color.a;
            float colorR = (float)color.r / 255 * color.a;

            float frameB = (float)frame[index + 0] / 255 * (255 - color.a);
            float frameG = (float)frame[index + 1] / 255 * (255 - color.a);
            float frameR = (float)frame[index + 2] / 255 * (255 - color.a);

            frame[index + 0] = (byte)(colorB + frameB);
            frame[index + 1] = (byte)(colorG + frameG);
            frame[index + 2] = (byte)(colorR + frameR);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void WriteColorToFrame(ref Pixel color, int x, int y)
        {
            int index = y * stride + (x * 3);

            float colorB = (float)color.b / 255 * color.a;
            float colorG = (float)color.g / 255 * color.a;
            float colorR = (float)color.r / 255 * color.a;

            float frameB = (float)frame[index + 0] / 255 * (255 - color.a);
            float frameG = (float)frame[index + 1] / 255 * (255 - color.a);
            float frameR = (float)frame[index + 2] / 255 * (255 - color.a);

            frame[index + 0] = (byte)(colorB + frameB);
            frame[index + 1] = (byte)(colorG + frameG);
            frame[index + 2] = (byte)(colorR + frameR);
        }
        internal void MarkDirty()
        {
            baseImageDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinMaxExclusive(float x, float y, float min, float max)
        {
            return x >= min && x < max && y >= min && y < max;
        }
    }
}

