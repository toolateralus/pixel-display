using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Sprite : Component
    {
        public Bitmap? image; 
        public Vec2 size = new Vec2();
        public Color[,] colorData;

        public bool isCollider = false;
        public bool isSolidColor = true;
        public bool dirty = false; 

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

        public override void Update()
        {
            if (!isSolidColor && image is not null && dirty)
            {
                colorData = BitmapAsset.ColorArrayFromBitmap(image);
            }
        }
        public override void Awake()
        {
            base.Awake();

        }
        public Sprite()
        {

        }
    }

    public class Text : Component
    {

        public string text = "a";
        public FontAsset asset;


        private List<BitmapAsset> charImgAssets = new(); 
        private List<Bitmap> chars = new();
        private Dictionary<Bitmap, Vec2> characters = new();
        private List<Sprite> sprites = new(); 
        public override void Awake()
        {
            base.Awake();
            for (int i = 0; i < 25; i++)
            {
                if(!AssetLibrary.TryFetch($"bit{i}", out BitmapAsset value)) return;
                charImgAssets.Add(value);
            }
            foreach (var file in charImgAssets)
            {
                if (file == null) continue;
                chars.Add(file.bitmap);
            }
            asset = FontAssetFactory.CreateFont(0, 23, chars.ToArray());
            AssetPipeline.Register(typeof(FontAsset), asset);
            characters = FontAssetFactory.ToString(asset, text);
            for (int i = 0; i < characters.Count; i++)
            {
                Sprite sprite = new()
                {
                    image = chars[i],
                    dirty = true,
                    size = new Vec2(chars[i].Width, chars[i].Height),
                    isSolidColor = false
                };
                sprites.Add(sprite);
            }
            
        }
        
        public override void FixedUpdate(float delta)
        {
         
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
