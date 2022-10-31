using Color = System.Drawing.Color;

namespace PixelRenderer.Components
{
    public class Sprite : Component
    {
        public Vec2 size = new Vec2();
        public Color[,] colorData;
        public bool isCollider = false;
        public Sprite(Vec2 size, Color color, bool isCollider)
        {
            colorData = new Color[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                this.colorData[x, y] = color;
            }
            this.size = size;
            this.isCollider = isCollider;
        }
        public Sprite(Color[,] colorData)
        {
            this.colorData = colorData;
        }
        public Sprite(Vec2 size)
        {
            this.size = size;
        }
        public Sprite(Vec2 size, bool isCollider)
        {
            this.size = size;
            this.isCollider = isCollider;
        }
        public Sprite()
        {

        }
    }
}
