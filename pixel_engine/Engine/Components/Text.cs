using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets; 

namespace pixel_renderer
{
    public class Text : Component
    {

        public string text = "TextRendering";
        public FontAsset asset;


        private List<Sprite> sprites = new List<Sprite>();
        private List<BitmapAsset> charImgAssets = new();
        private List<Bitmap> chars = new();


        public override void Awake()
        {
            if (!Enabled) return; 
            if (asset == null)
            {
                // first try load, then create fallback.
                if (!Library.Fetch<FontAsset>(out List<object> fontAssetObjects)) return;

            }
            foreach (var x in asset.characters)
            {
                var bmp = x.Value;
                Sprite sprite = new()
                {
                    image = bmp,
                    dirty = true,
                    size = new Vec2(bmp.Width, bmp.Height),
                };
                parent.AddComponent(sprite);
            }
            var components = parent.GetComponents<Sprite>();
            foreach (var component in components)
            sprites.Add(component ?? new(0, 0));
        }
        public void CreateFontAsset()
        {
            if (!Library.Fetch<BitmapAsset>(out List<object> imageObjects)) return;

            foreach (var img in imageObjects) charImgAssets.Add((BitmapAsset)img);

            foreach (var file in charImgAssets)
            {
                if (file == null) continue;
                chars.Add(file.RuntimeValue);
            }

            asset = FontAssetFactory.CreateFont(0, 23, chars.ToArray());

            Library.Register(typeof(FontAsset), asset);

            var imgs = FontAsset.GetCharacterImages(asset, text);

            for (int i = 0; i < imgs.Count; i++)
                asset.characters.Add(text[i], imgs[i]);

        }

        public override void FixedUpdate(float delta)
        {
            return; 
            var positions = FontAsset.GetCharacterPosition(asset);
            int i = 0;
            foreach (var x in sprites)
            {
                if (positions.Count <= i) continue; 
                x.parent.position = positions[i];
                ++i; 
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
