using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;

namespace pixel_renderer
{
    public class StageRenderInfo
    {
        public List<SpriteInfo> spriteInfos = new List<SpriteInfo>();

        public StageRenderInfo(Stage stage)
        {
           Refresh(stage);
        }
        public void Refresh(Stage stage)
        {
            var sprites = stage.GetSprites();
            
            int spriteCt = sprites.Count();
            int infoCount = spriteInfos.Count; 
            if (spriteCt != infoCount)
            {
                for (int i = infoCount; i < spriteCt; ++i)
                    spriteInfos.Add(new());
                for (int i = infoCount; i > spriteCt; --i)
                    spriteInfos.RemoveAt(0);
            }
            for (int i = 0; i < sprites.Count(); ++i)
            {
                Sprite sprite = sprites.ElementAt(i);
                spriteInfos[i].Set(sprite);
            }
        }
    }
    public class SpriteInfo
    {
        public Vector2 pos = new();
        public float rotation; 
        public Vector2 size = new();
        public Vector2 viewportOffset = new();
        public Vector2 viewportScale = new();
        public float camDistance = new();
        public TextureFiltering filtering = new();
        public JImage image = new();
        public Vector2 colorDataSize;

        public void Set(Sprite sprite)
        {
            rotation = sprite.rotation; 
            pos = sprite.node.Position;
            size = sprite.size;
            viewportOffset = sprite.viewportOffset;
            viewportScale = sprite.viewportScale;
            image = sprite.texture.GetImage();
            filtering = sprite.textureFiltering;
            camDistance = sprite.camDistance;
            colorDataSize = image.Size;
            size.MakeDivideSafe();
        }
        public Vector2 ViewportToColorPos(Vector2 spriteViewport) => 
            ((spriteViewport + viewportOffset) * viewportScale).Wrapped(Vector2.One) * colorDataSize;
        internal Vector2 GlobalToViewport(Vector2 global) =>
            (global - pos) / size;
        public void SetColorData(Vector2 size, byte[] data)
        {
            image = new(size, data);
            colorDataSize = new(size.X, size.Y);
        }
        public SpriteInfo() { }
      
    }
}
