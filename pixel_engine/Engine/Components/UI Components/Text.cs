using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;
using System.Security.Policy;

namespace pixel_renderer
{
    public enum InterpolationType { Linear, Exponential, ExponentialSqrd}

    public class Text : UIComponent
    {
        public Dictionary<char , JImage> font = new();
        /// <summary>
        /// the bounding box of the text element
        /// </summary>


        public BoundingBox2D bounds = new(new(0,0), new(15,5));
        public Curve posCurve = Curve.Linear(new Vector2(), new Vector2(), 15);

        const string alphabet = "abcdefghijklmnopqrstuvwxyz"; 

        public override void Awake()
        {
            lock(font)
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
            JImage.Concat(font.Values, bounds, posCurve, posCurve);
        }
        public override void Update()
        {


        }
        public override void Draw(RendererBase renderer)
        {
          
        }
    }
}
