namespace pixel_renderer
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

    public class CRenderer : RendererBase
    {
        public static Bitmap? bmp_cached = null;
        public static IEnumerable<Sprite> sprites_cached;
        public override Bitmap Draw()
        {
            sprites_cached = Runtime.Instance.stage.GetSprites();
            foreach (var sprite in sprites_cached)
            {
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        int offsetX =(int)sprite.parentNode.position.x + x;
                        int offsetY =(int)sprite.parentNode.position.y + y;

                        if (offsetX is < 0 or >= Settings.ScreenW
                            || offsetY is < 0 or >= Settings.ScreenH) continue;

                        bmp_cached.SetPixel(offsetX, offsetY, sprite.colorData[x, y]);
                    }
            }
            return bmp_cached;
        }
        public override void Render(Image output) =>  CBit.Render(ref bmp_cached, output);
        public override void Dispose()  => bmp_cached = Runtime.Instance.stage.Background.Clone() as Bitmap ?? FallBack.Clone() as Bitmap;
    }               
    }

