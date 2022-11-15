﻿namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;

    public static class Rendering
    {
        /// <summary>
        /// Game = Build;
        /// Scene = Inspector;
        /// Off = Off; 
        /// Controlled and read externally, serves as a reference to what is currently being rendered; 
        /// </summary>
        public static RenderState State = RenderState.Game;
        public static Queue<Bitmap> FrameBuffer = new Queue<Bitmap>();
        public static double FrameRate()
        {
            Runtime env = Runtime.Instance;
            var lastFrameTime = env.lastFrameTime;
            var frameCount = env.frameCount;
            var frameRate = Math.Floor(1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds * frameCount);
               
            return frameRate;
        }
        static Runtime runtime => Runtime.Instance;
        public static void Render(Image output)
        {
            // if we could avoid cloning this object
            // and instead cache the original colors during changes and rewrite them back
            // it would save a very significant amount of memory
            // and CPU

            var clonedBackground = (Bitmap)runtime.stage.Background.Clone();
            var frame = Draw(clonedBackground);
            DrawToImage(ref frame, output);
        }
        private static Bitmap Draw(Bitmap frame)
        {
            Stage stage = Runtime.Instance.stage;
            foreach (var node in stage.Nodes)
            {
                var sprite = node.GetComponent<Sprite>();
                if (sprite is null) continue;

                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        var offsetX = node.position.x + x;
                        var offsetY = node.position.y + y;
                        if (offsetX < 0) continue;
                        if (offsetY < 0) continue;

                        if (offsetX >= Constants.ScreenWidth) continue;
                        if (offsetY >= Constants.ScreenHeight) continue;

                        var color = sprite.colorData[x, y];
                        var position = new Vec2((int)offsetX, (int)offsetY);

                        var pixelOffsetX = (int)position.x;
                        var pixelOffsetY = (int)position.y;

                        frame.SetPixel(pixelOffsetX, pixelOffsetY, color);
                    }
            }
            return frame;
        }
        private static void DrawToImage(ref Bitmap inputFrame, Image renderImage)
        {
            CBit.Convert(inputFrame, renderImage);
        }
    }

}