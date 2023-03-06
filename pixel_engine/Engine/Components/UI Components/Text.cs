using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;
using System.Security.Policy;

namespace pixel_renderer
{

    public class Text : UIComponent
    {
        public Dictionary<char , JImage> font = new();
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
                    font.Add(alphabet[i], image);
                }
            }
        }
        public override void Update()
        {


        }
        public override void Draw(RendererBase renderer)
        {
            foreach (var x in font)
                DrawImage(renderer, x.Value);
        }
    }
}
