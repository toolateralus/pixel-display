namespace pixel_renderer
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

    public class CRenderer : RendererBase
    {
        private Bitmap bmp_cached = new(1,1);
        private IEnumerable<Sprite>? sprites_cached = null;

        private Bitmap? _background;
        public Bitmap Background
        {
            get
            {
                _background ??= Runtime.Instance.GetStage().backgroundImage;
                return _background;
            }
        }
        public override void Dispose()
        {
            if (Background is not null)
            {
                bmp_cached = (Bitmap)Background.Clone();
                return;
            }
            bmp_cached = (Bitmap)FallBack.Clone();
        }
        public Bitmap _Draw()
        {
            sprites_cached = Runtime.Instance.GetStage().GetSprites();
            foreach (Sprite sprite in sprites_cached)
            {
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        int offsetX = (int)sprite.parent.position.x + x;
                        int offsetY = (int)sprite.parent.position.y + y;

                        if (offsetX is < 0 or >= Constants.ScreenW
                            || offsetY is < 0 or >= Constants.ScreenH) continue;

                        bmp_cached.SetPixel(offsetX, offsetY, sprite.colorData[x, y]);
                    }
            }
            return bmp_cached;
        }
        public override Bitmap Draw()
        {
            //CBit.Draw(Runtime.Instance.GetStage().GetSprites(), bmp_cached);
            return _Draw();
        }

        public override void Render(Image destination) => CBit.Render(ref bmp_cached, destination);
    }
}