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
            sprites_cached = Runtime.Instance.GetStage().GetSprites();
            foreach (Sprite sprite in sprites_cached)
            {
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        int offsetX = (int)sprite.parent.position.x + x;
                        int offsetY = (int)sprite.parent.position.y + y;

                        if (offsetX is < 0 or >= Settings.ScreenW
                            || offsetY is < 0 or >= Settings.ScreenH) continue;

                        bmp_cached.SetPixel(offsetX, offsetY, sprite.colorData[x, y]);
                    }
            }
            return bmp_cached;
        }
        public override void Render(Image output) =>  CBit.Render(ref bmp_cached, output);
          
        public Bitmap Background 
        {
            get 
            {
                if (_background is null)
                    _background = Runtime.Instance.GetStage().Background.Colors.ToBitmap(); 
                return _background; 
            }
        }
        private Bitmap _background; 

        public override void Dispose()
        {
            if (Background is not null)
            {
                bmp_cached = Background.Clone() as Bitmap;
                return;
            }
            bmp_cached = FallBack.Clone() as Bitmap;
        }
    }
}

