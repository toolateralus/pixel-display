namespace pixel_renderer
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
            var frameRate =
                Math.Floor(1 /
                TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds
                * frameCount);
            return frameRate;
        }
        static Runtime runtime => Runtime.Instance;
        public static void Render(Image output)
        {
            var player = runtime.stage.FindNode("Player");
            var cam = player.GetComponent<Camera>();
            var frame = Draw(cam, (Bitmap)runtime.stage.Background.Clone());
            Insert(frame);
            var renderFrame = FrameBuffer.First();
            DrawToImage(ref renderFrame, output);
        }

        private static Bitmap Draw(Camera camera, Bitmap frame)
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

                        if (offsetX >= Constants.screenWidth) continue;
                        if (offsetY >= Constants.screenHeight) continue;

                        var color = sprite.colorData[x, y];
                        var position = new Vec2((int)offsetX, (int)offsetY);

                        var pixelOffsetX = (int)position.x;
                        var pixelOffsetY = (int)position.y;

                        frame.SetPixel(pixelOffsetX, pixelOffsetY, color);
                    }
            }
            return frame;
        }
        private static void Insert(Bitmap inputFrame)
        {
            if (FrameBuffer.Count > 0) FrameBuffer.Dequeue();
            FrameBuffer.Enqueue(inputFrame);
        }
        private static void DrawToImage(ref Bitmap inputFrame, Image renderImage)
        {
            CBitmap.Convert(inputFrame, renderImage);
        }
    }

}