namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Bitmap = System.Drawing.Bitmap;

    public enum RenderState { Game, Scene, Off }
    public  class Rendering
    {
        public static RenderState State = RenderState.Game;
        public static double FrameRate()
        {
            Runtime env = Runtime.Instance;
            var lastFrameTime = env.lastFrameTime;
            var frameCount = env.frameCount;
            var frameRate = Math.Floor(1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds * frameCount);

            return frameRate;
        }

        private static Runtime runtime => Runtime.Instance;
        private static Bitmap? bmp_cached = null; 
        private static IEnumerable<Sprite> sprites_cached; 

        private static Bitmap fallback;
        private static Bitmap FallBack
        {
            get => fallback ??= new(256, 256);
        }

        public static void Clear() =>  bmp_cached = runtime.stage.Background.Clone() as Bitmap ?? FallBack.Clone() as Bitmap;
          
        public static void Render(System.Windows.Controls.Image output)
        {
            if (runtime.stage is null)
            {
                State = RenderState.Off;
                runtime.IsRunning = false;
                return;
            }
            sprites_cached = Runtime.Instance.stage.GetSprites();
            Clear(); 
            var frame = Draw(bmp_cached, sprites_cached);
            CBit.Render(ref frame, output); 
        }

        /// <summary>
        /// Very rudimentary way to draw sprite data over top a background.  
        /// Rather inefficient yet absolutely sufficient for light use, and easy to modify.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="sprites"></param>
        /// <returns></returns>
        private static Bitmap Draw(Bitmap bmp, IEnumerable<Sprite> sprites)
        {
            foreach (var sprite in sprites)
            {
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        var offsetX = sprite.parentNode.position.x + x;
                        var offsetY = sprite.parentNode.position.y + y;

                        if (offsetX is < 0 or >= Settings.ScreenW
                            || offsetY is < 0 or >= Settings.ScreenH) continue;
                        var color = sprite.colorData[x, y];

                        bmp.SetPixel((int)offsetX, (int)offsetY, color);
                    }
            }
            return bmp;
        }

        
        static string cachedGCValue = "";

        const int framesUntilGC_Check = 600;
        private static int framesSinceGC_Check = 0;

        public static string GetGCStats()
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
    }

