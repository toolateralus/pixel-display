using pixel_renderer.Engine.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace pixel_renderer
{
    public class StageRenderInfo
    {
        public int Count => spritePositions.Count;

        public List<Vec2> spritePositions= new ();
        public List<Vec2> spriteSizeVectors = new();
        public List<Vec2> spriteVPOffsetVectors = new();
        public List<Vec2> spriteVPScaleVectors = new();
        public List<float> spriteCamDistances = new();
        public List<TextureFiltering> spriteFiltering = new();
        public List<byte[]> spriteColorData = new();

        public StageRenderInfo(Stage stage)
        {
           Refresh(stage);
        }
        public void SetSprite(Sprite sprite, int index)
        {
            sprite.parent.Position = spritePositions[index];
            sprite.ColorData = spriteColorData[index];
            sprite.size = spriteSizeVectors[index];
            sprite.viewportOffset = spriteVPOffsetVectors[index];
            sprite.viewportScale = spriteVPScaleVectors[index];
            sprite.textureFiltering = spriteFiltering[index];
            sprite.camDistance = spriteCamDistances[index];
        }
        public void Refresh(Stage stage)
        {
            var sprites = stage.GetSprites();
            
            int spriteCt = sprites.Count();
            int spritePosCt = Count; 
            if (spriteCt != spritePosCt)
            {
                for (int i = Count; i < spriteCt; ++i)
                    addMemberOnTop();
                for (int i = Count; i > spriteCt; --i)
                    removeFirst();
            }
            for (int i = 0; i < sprites.Count(); ++i)
            {
                Sprite sprite = sprites.ElementAt(i);
                spritePositions[i] = sprite.parent.Position;
                spriteSizeVectors[i] = sprite.size;
                spriteVPOffsetVectors[i] = sprite.viewportOffset;
                spriteVPScaleVectors[i] = sprite.viewportScale;
                spriteColorData[i] = sprite.ColorData;
                spriteFiltering[i] = sprite.textureFiltering;
                spriteCamDistances[i] = sprite.camDistance;
            }
            void addMemberOnTop()
            {
                spritePositions.Add(Vec2.zero);
                spriteSizeVectors.Add(Vec2.zero);
                spriteVPOffsetVectors.Add(Vec2.zero);
                spriteVPScaleVectors.Add(Vec2.zero);
                spriteFiltering.Add(0);
                spriteColorData.Add(Array.Empty<byte>());
                spriteCamDistances.Add(1f);
            }
            void removeFirst()
            {
                spritePositions.RemoveAt(0);
                spriteSizeVectors.RemoveAt(0);
                spriteVPOffsetVectors.RemoveAt(0);
                spriteVPScaleVectors.RemoveAt(0);
                spriteFiltering.RemoveAt(0);
                spriteColorData.RemoveAt(0);
                spriteCamDistances.RemoveAt(0);

            }
        }
    }
}
