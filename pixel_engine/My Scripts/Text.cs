using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;

namespace pixel_renderer
{
    public class Text : UIComponent
    {
        public Dictionary<char, (JImage image, Vector2 scale)> font = new();
        public Dictionary<Sprite, char> nodes = new(); 
        /// <summary>
        /// the bounding box of the text element
        /// </summary>
        public BoundingBox2D bounds;

        const string alphabet = "abcdefghijklmnopqrstuvwxyz"; 

        public override void Awake()
        {
            for (int i = 0; i < 3; ++i)
            {
                var meta = AssetLibrary.FetchMetaRelative($"\\Assets\\Fonts\\font{i}.bmp");
                if (meta != null)
                {
                    Bitmap bmp = new(meta.Path);
                    JImage image = new(bmp);
                    
                    Node node = Rigidbody.Standard("Font Test Node.");


                    node.Transform = Matrix3x2.CreateScale(35);
                    if (!node.TryGetComponent(out Sprite sprite)) 
                        continue;

                    sprite.texture.SetImage(image);

                    var scale = sprite.texture.scale; 


                    font.Add(alphabet[i], (image, scale));
                    nodes.Add(sprite, alphabet[i]);
                }
            }
        }
        public override void Update()
        {
            foreach (var node in nodes.Keys)
                node.Position = Position + Vector2.One * 15; 
        }



    }
}
