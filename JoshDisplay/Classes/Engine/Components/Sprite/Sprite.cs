﻿using Color = System.Drawing.Color;

namespace pixel_renderer
{ 
    public class Sprite : Component
    {
        public Vec2 size = new Vec2();
        public Color[,] colorData;
        public bool isCollider = false;
        public override string UUID { get; set; }
        public override string Name { get; set; }
        public Sprite(Vec2 size, Color color, bool isCollider)
        {
            DrawSquare(size, color, isCollider);
        }

        public void DrawSquare(Vec2 size, Color color, bool isCollider)
        {
            colorData = new Color[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    colorData[x, y] = color;
                }
            this.size = size;
            this.isCollider = isCollider;
        }
        public void DrawSquare(Vec2 size, Color[,] color, bool isCollider)
        {
            colorData = color;
            this.size = size;
            this.isCollider = isCollider;
        }



        public Sprite()
        {

        }
    }
}
