using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Controls;

namespace pixel_renderer
{

    public class SpriteCamera : UIComponent
    {
        [JsonProperty] Vec2 viewportPosition = new(0,0);
        [JsonProperty] Vec2 viewportSize = new(1,1);
        [JsonProperty] public float angle = 0f;
        [JsonProperty] public DrawingType bgDrawingType = DrawingType.Wrapped;
        float[,] zBuffer = new float[0,0];

        public Vec2 GlobalToViewport(Vec2 global) => ((global - Center).Rotated(angle) + bottomRightCornerOffset) / Size.GetDivideSafe();
        public Vec2 ViewportToGlobal(Vec2 vpPos) => (vpPos * Size - bottomRightCornerOffset).Rotated(-angle) + Center;
        public override void Draw(Bitmap bmp)
        {
            IEnumerable<Sprite> sprites = Runtime.Instance.GetStage().GetAllComponents<Sprite>();
            if (bmp.Width != zBuffer.GetLength(0) ||
                bmp.Height != zBuffer.GetLength(1))
                zBuffer = new float[bmp.Width, bmp.Height];

            Array.Clear(zBuffer);

            DrawBackground(bmp);

            foreach (Sprite sprite in sprites)
                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        Vec2 viewportPos = GlobalToViewport(sprite.parent.position + new Vec2(x, y));

                        viewportPos *= viewportSize;
                        viewportPos += viewportPosition;

                        if (viewportPos.x < 0 || viewportPos.x >= 1)
                            continue;

                        if (viewportPos.y < 0 || viewportPos.y >= 1) 
                            continue;

                        int screenPosX = (int)(viewportPos.x * bmp.Width);
                        int screenPosY = (int)(viewportPos.y * bmp.Height);

                        if (sprite.camDistance <= zBuffer[screenPosX, screenPosY]) 
                            continue;

                        zBuffer[screenPosX, screenPosY] = sprite.camDistance;

                        bmp.SetPixel(screenPosX, screenPosY, sprite.colorData[x, y]);
                    }
        }

        private void DrawBackground(Bitmap bmp)
        {
            if (bgDrawingType == DrawingType.None) return;
            Bitmap bg = Runtime.Instance.GetStage().backgroundImage;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Vec2 bgSize = new Vec2(bg.Width, bg.Height);
                    Vec2 bmpSize = new Vec2(bmp.Width, bmp.Height);

                    Vec2 viewportPos = new Vec2(x,y) / bmpSize.GetDivideSafe();
                    Vec2 globalPos = ViewportToGlobal(viewportPos);
                    Vec2 bgViewport = (globalPos / bgSize.GetDivideSafe());
                    if (bgDrawingType == DrawingType.Wrapped)
                    {
                        bgViewport += new Vec2(1, 1);
                        Vec2 wrappedBgViewport = new(bgViewport.x - (int)bgViewport.x, bgViewport.y - (int)bgViewport.y);
                        Vec2 bgPos = wrappedBgViewport * bgSize;
                        bmp.SetPixel(x, y, bg.GetPixel((int)bgPos.x, (int)bgPos.y));
                    }
                    if (bgDrawingType == DrawingType.Clamped)
                    {
                        bgViewport.Clamp(Vec2.zero, Vec2.one);
                        Vec2 bgPos = (bgViewport * (bgSize - Vec2.one)).Clamped(Vec2.zero, bmpSize);
                        bmp.SetPixel(x, y, bg.GetPixel((int)bgPos.x, (int)bgPos.y));
                    }
                }
            }
        }

        public enum DrawingType { Wrapped, Clamped, None}
    }
}
