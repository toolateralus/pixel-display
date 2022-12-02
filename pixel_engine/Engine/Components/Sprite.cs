using System;
using System.Drawing;
using pixel_renderer;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Sprite : Component
    {
        public Bitmap? image;
        public Vec2 size = new Vec2();
        public Color[,] colorData;

        public bool isCollider = false;
        public bool dirty = false;

        dynamic? cachedColor = new Color();

        public override void Awake()
        {


        }
        public override void Update()
        {
     
        }
      
        public void Randomize()
        {
            int x = (int)size.x;
            int y = (int)size.y;
            cachedColor = this.colorData;
            var colorData = new Color[x, y];
            for (int j = 0; j < y; j++)
                for (int i = 0; i < x; i++)
                    colorData[i, j] = JRandom.Color();
            DrawSquare(size, colorData, isCollider);
        }
        public void RestoreCachedColor(bool nullifyCache)
        {
            if (cachedColor == null) Randomize();
            DrawSquare(size, cachedColor, isCollider);
            if (nullifyCache) cachedColor = null;
        }

        /// <summary>
        /// caches the current color data of the sprite and sets every pixel in the color data to the one passed in.
        /// </summary>
        /// <param name="borderColor"></param>
        public void Highlight(Color borderColor)
        {
            int x = (int)size.x;
            int y = (int)size.y;
            cachedColor = this.colorData;
            var colorData = new Color[x, y];
            for (int i = 0; i < y; i++)
                for (int j = 0; j < x; j++)
                {
                    if (colorData.Length < i && colorData.Length < j)
                        if (j == 0 || j == x && i == 0 || i == y)
                            colorData[i, j] = borderColor;

                }
            DrawSquare(size, colorData, isCollider);
        }
        
        public void DrawSquare(Vec2 size, Color[,] color, bool isCollider)
        {
            colorData = color;
            this.size = size;
            this.isCollider = isCollider;
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

        public static Bitmap SolidColorBitmap(Vec2 size, Color color)
        {
            int x = (int)size.x;
            int y = (int)size.y; 
            
            var bitmap = new Bitmap(x, y);

            for(int i = 0; i < x ; i++)
                for(int j = 0; j < x ; j++)
                    bitmap.SetPixel(i, j, color);

            return bitmap;
        }
        public static Color[,] SolidColorSquare(Vec2 size, Color color)
        {
            var colorData = new Color[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    colorData[x, y] = color;
            return colorData; 
        }

        public Sprite()
        {
            colorData = new Color[0,0]; 
        }
        public Sprite(int x, int y)
        {
            Vec2 size = new(x, y);
            this.size = size;
        }
        public Sprite(Vec2 size, Color color, bool isCollider)
        {
            DrawSquare(size, color, isCollider);
        }
        
    }


}
