using System.Collections.Generic;
using System.Drawing;

namespace pixel_renderer.Engine.Renderer
{
    public class SpriteCamera
    {
        Vec2 halfSize;
        public Vec2 Size { get => halfSize * 2; set => halfSize = value * 0.5f; }
        public Vec2 center;
        public float angle;

        public Vec2 GlobalToViewport(Vec2 global) => ((global - center).Rotated(angle) + halfSize) / Size;
        public void Draw(List<Sprite> sprites, Bitmap bmp)
        {
            foreach (Sprite sprite in sprites)
            {
                for (int x = 0; x < sprite.size.x; x++)
                {
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        Vec2 viewportPos = GlobalToViewport(sprite.parent.position + new Vec2(x, y));

                        if (viewportPos.x < 0 || viewportPos.x >= 1) continue;
                        if (viewportPos.y < 0 || viewportPos.y >= 1) continue;

                        int screenPosX = (int)(viewportPos.x * bmp.Width);
                        int screenPosY = (int)(viewportPos.y * bmp.Height);

                        bmp.SetPixel(screenPosX, screenPosY, sprite.colorData[x, y]);
                    }
                }
            }
        }
        public SpriteCamera(Vec2 size, Vec2 center, float angle = 0f)
        {
            Size = size;
            this.center = center;
            this.angle = angle;
        }
    }
}
