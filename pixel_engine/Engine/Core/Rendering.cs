namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Bitmap = System.Drawing.Bitmap;

    public enum RenderState { Game, Scene, Off }
    public static class Rendering
    {
        /// <summary>
        /// Game = Build;
        /// Scene = Inspector;
        /// Off = Off; 
        /// Controlled and read externally, serves as a reference to what is currently being rendered; 
        /// </summary>
        public static RenderState State = RenderState.Game;
        public static double FrameRate()
        {
            Runtime env = Runtime.Instance;
            var lastFrameTime = env.lastFrameTime;
            var frameCount = env.frameCount;
            var frameRate = Math.Floor(1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds * frameCount);

            return frameRate;
        }
        static Runtime runtime => Runtime.Instance;

        static Bitmap fallback;
        public static Bitmap FallBack
        {
            get => fallback ??= new(256, 256);
        }

        public static void Render(System.Windows.Controls.Image output)
        {
            // if we could avoid cloning this object
            // and instead cache the original colors during changes and rewrite them back
            // it would save a very significant amount of memory
            // and CPU

            if (runtime.stage is null)
            {
                // render loop shutoff
                runtime.IsRunning = false;
                return;
            }
            var background = runtime.stage.Background ?? FallBack;
            var frame = Draw(background);
            CBit.Render(ref frame, output); 

        }

        [DllImport("PIXELRENDERER", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetHBITMAP(IntPtr intPtr, byte r, byte g, byte b);

        private static Bitmap Draw(Bitmap bmp)
        {
            //unsafe bitmap draw[C# native code]
            // CBit.Draw(runtime.stage, bmp);
            //return bmp;

            // from DLL (not importing, maybe needs library for GetDIBits/SetDIBits) [C++ native code] )
            //var hbit = GetHBITMAP(frame.GetHbitmap(), 255, 255, 255);
            //frame = Image.FromHbitmap(hbit);
            //return frame;

            // NORMAL RENDERING BELOW;
            Stage stage = Runtime.Instance.stage;
            Sprite sprite_ = new(0, 0);
            IEnumerable<Sprite> sprites = from Node node in stage.Nodes
                                                               where  node.TryGetComponent(out sprite_)
                                                               where sprite_.isCollider
                                                               select  sprite_ ; 
            
            foreach (var sprite in sprites)
            {
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        var offsetX = sprite.parentNode.position.x + x;
                        var offsetY = sprite.parentNode.position.y + y;

                        if (offsetX is < 0 or >= Settings.ScreenWidth 
                            || offsetY is < 0 or >= Settings.ScreenHeight) continue;
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

