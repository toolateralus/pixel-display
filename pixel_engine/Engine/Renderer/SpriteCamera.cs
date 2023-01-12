using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;

namespace pixel_renderer
{
    public class SpriteCamera : Component
    {
        [JsonProperty] Vec2 halfSize;
        [JsonProperty] Vec2 viewportPosition = new(0,0);
        [JsonProperty] Vec2 viewportSize = new(1, 1);
        [JsonProperty] public float angle;
        float[,] zBuffer = new float[0,0];
        public Vec2 Size { get => halfSize * 2; set => halfSize = value * 0.5f; }
        public Vec2 Center { get => parent.position; set => parent.position = value; }

        public Vec2 GlobalToViewport(Vec2 global) => ((global - Center).Rotated(angle) + halfSize) / Size;
        public void Draw(IEnumerable<Sprite> sprites, Bitmap bmp)
        {
            if (bmp.Width != zBuffer.GetLength(0) ||
                bmp.Height != zBuffer.GetLength(1))
                zBuffer = new float[bmp.Width, bmp.Height];
            Array.Clear(zBuffer);

            foreach (Sprite sprite in sprites)
            {
                for (int x = 0; x < sprite.size.x; x++)
                {
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        Vec2 viewportPos = GlobalToViewport(sprite.parent.position + new Vec2(x, y));
                        viewportPos *= viewportSize;
                        viewportPos += viewportPosition;

                        if (viewportPos.x < 0 || viewportPos.x >= 1) continue;
                        if (viewportPos.y < 0 || viewportPos.y >= 1) continue;

                        int screenPosX = (int)(viewportPos.x * bmp.Width);
                        int screenPosY = (int)(viewportPos.y * bmp.Height);

                        if (sprite.camDistance <= zBuffer[screenPosX, screenPosY]) continue;
                        zBuffer[screenPosX, screenPosY] = sprite.camDistance;

                        bmp.SetPixel(screenPosX, screenPosY, sprite.colorData[x, y]);
                    }
                }
            }
        }
    }
}
