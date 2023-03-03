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
        public Vector2 scale;
        public Matrix3x2 Transform;
        public Vector2 viewportOffset = new();
        public Vector2 viewportScale = new();
        public float camDistance = new();
        public TextureFiltering filtering = new();
        public JImage image = new();
        public Vector2 colorDataSize;

        public void Set(Sprite sprite)
        {
            viewportOffset = sprite.viewportOffset;
            viewportScale = sprite.viewportScale.GetDivideSafe();
            camDistance = sprite.camDistance;
            
            image = sprite.texture.GetImage();
            colorDataSize = image.Size;
            filtering = sprite.textureFiltering;
            
            Transform = sprite.Transform;
            scale = sprite.Scale;
        }
        public Vector2 ViewportToColorPos(Vector2 spriteViewport) => 
            ((spriteViewport + viewportOffset) * viewportScale).Wrapped(Vector2.One) * colorDataSize;
        internal Vector2 GlobalToViewport(Vector2 global) =>
            (global - Transform.Translation) / scale;
        public void SetColorData(Vector2 size, byte[] data)
        {
            image = new(size, data);
            colorDataSize = new(size.X, size.Y);
        }
        public SpriteInfo() { }
      
    }
}
