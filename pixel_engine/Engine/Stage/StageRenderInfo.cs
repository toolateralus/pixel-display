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
        public Vector2 LocalToViewport(Vector2 local) => (local + viewportOffset) * viewportScale;
        public Vector2 LocalToColorPosition(Vector2 local) => ViewportToColorPosition(LocalToViewport(local));
        public Vector2 ViewportToColorPosition(Vector2 viewport)
        {
            viewport.X += 0.5f;
            viewport.Y += 0.5f;
            return viewport.Wrapped(Vector2.One) * colorDataSize;
        }

        internal Vector2 GlobalToLocal(Vector2 global)
        {
            Matrix3x2.Invert(Transform, out var inverted);
            return Vector2.Transform(global, inverted);
        }

        public void SetColorData(Vector2 size, byte[] data)
        {
            image = new(size, data);
            colorDataSize = new(size.X, size.Y);
        }
        public Vector2[] GetCorners()
        {
            Vector2 topLeft = Vector2.Transform(new Vector2(-0.5f, -0.5f), Transform);
            Vector2 topRight = Vector2.Transform(new Vector2(0.5f, -0.5f), Transform);
            Vector2 bottomRight = Vector2.Transform(new Vector2(0.5f, 0.5f), Transform);
            Vector2 bottomLeft = Vector2.Transform(new Vector2(-0.5f, 0.5f), Transform);

            var vertices = new Vector2[]
            {
                    topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft,
            };

            return vertices;
        }
        public SpriteInfo() { }
      
    }
}
