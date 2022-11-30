﻿namespace pixel_renderer
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

    public class CRenderer : RendererBase
    {
        Bitmap? bmp_cached = null;
        IEnumerable<Sprite>? sprites_cached = null; 
        public override Bitmap Draw()
        {
            sprites_cached = Runtime.Instance.stage.GetSprites();
            foreach (Sprite sprite in sprites_cached)
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
          
        public Bitmap background 
        {
            get 
            {
                if (_background is null)
                    _background = Runtime.Instance.stage.Background.Colors.ToBitmap(); 
                return _background; 
            }
        }
        private Bitmap _background; 

        public override void Dispose()
        {
            if (background is not null)
            {
                bmp_cached = background.Clone() as Bitmap;
                return;
            }
            bmp_cached = FallBack.Clone() as Bitmap;
        }
    }
}

