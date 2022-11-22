using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Policy;
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

        public Sprite(Vec2 size, Color color, bool isCollider)
        {
            DrawSquare(size, color, isCollider);
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
                        if (j == 0 || j == x &&  i == 0 || i == y)
                            colorData[i,j] = borderColor;
                    
                }
            DrawSquare(size, colorData, isCollider);
        }

        public void DrawSquare(Vec2 size, Color[,] color, bool isCollider)
        {
            colorData = color;
            this.size = size;
            this.isCollider = isCollider;
        }

        public override void Update()
        {
            if (image is not null && dirty)
            {
                size = new Vec2(image.Width, image.Height); 
                colorData = BitmapAsset.ColorArrayFromBitmap(image);
            }
        }
        public override void Awake()
        {
             

        }
        public Sprite()
        {

        }

        public Sprite(int x, int y)
        {
            Vec2 size = new(x, y);
            this.size = size; 
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
    }

    public class Text : Component
    {

        public string text = "a";
        public FontAsset asset;


        private List<BitmapAsset> charImgAssets = new(); 
        private List<Bitmap> chars = new();

        
        public override void Awake()
        {
            return; 
            if (!AssetLibrary.Fetch<BitmapAsset>(out List<object> imageObjects)) return; 

            foreach (var img in imageObjects) charImgAssets.Add((BitmapAsset) img);
           
            
            foreach (var file in charImgAssets)
            {
                if (file == null) continue;
                chars.Add(file.RuntimeValue);
            }

            asset = FontAssetFactory.CreateFont(0, 23, chars.ToArray());
            
            AssetLibrary.Register(typeof(FontAsset), asset);
            
            var imgs = FontAsset.GetCharacterImages(asset, text);

            for(int i = 0; i < imgs.Count; i++)
                asset.characters.Add(text[i], imgs[i]);
            

            foreach (var x in asset.characters)
            {
                var bmp = x.Value;
                Sprite sprite = new()
                {
                    image = bmp,
                    dirty = true,
                    size = new Vec2(bmp.Width, bmp.Height),
                };
                parentNode.AddComponent(sprite);
            }


        }
        
        public override void FixedUpdate(float delta)
        {
            var components = parentNode.GetComponents<Sprite>(); 
            if (components.Count > 0)
            {
                var positions = FontAsset.GetCharacterPosition(asset); 

                for (int i = 0; i < components.Count; i++)
                    components[i].parentNode.position = positions[i];
            }
        }

        public override void OnCollision(Rigidbody collider)
        {
        }

        public override void OnTrigger(Rigidbody other)
        {
        }

        public override void Update()
        {
        }
    }
 
    
}
