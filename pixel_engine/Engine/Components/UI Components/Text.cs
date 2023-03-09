using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;
using System.Security.Policy;
using System.Linq;
using System.Runtime.InteropServices;

namespace pixel_renderer
{
    public enum InterpolationType { Linear, Exponential, ExponentialSqrd}

    public class Text : UIComponent
    {
        public Dictionary<char , JImage> font_model = new();
        /// <summary>
        /// the bounding box of the text element
        /// </summary>


        public Vector2 start = new(0, 0);
        public Vector2 end = new(1, 1);

        public Curve posCurve;
        private JImage current;

        public string Content
        {
            get => content;
            set
            {
                content = value;
                RefreshCharacters();
            }
        }

        private string content = "abc";
        const string alphabet = "abcdefghijklmnopqrstuvwxyz"; 

        public override void Awake()
        {
            RefreshFont();
        }
        [Method]
        public void RefreshFont()
        {
            font_model.Clear();
            
            lock (font_model)
                for (int i = 0; i < 3; ++i)
                {
                    var meta = AssetLibrary.FetchMetaRelative($"\\Assets\\Fonts\\font{i}.bmp");
                    if (meta != null)
                    {
                        Bitmap bmp = new(meta.Path);
                        JImage image = new(bmp);
                        font_model.Add(alphabet[i], image);
                    }
                }
        }
        [Method]
        public void RefreshCharacters()
        {

            posCurve = Curve.Linear(start, end, 1, Content.Length);
            posCurve = Curve.Normalize(posCurve);
            var output = new List<JImage>();
            
            for (int i = 0; i < Content.Length; i++)
                if (font_model.ContainsKey(Content[i]))
                {
                    var img = font_model.ElementAt(i).Value;
                    output.Add(img);
                }

            current = JImage.Concat(output, posCurve);
        }
        public override void Update()
        {


        }
        public override void Draw(RendererBase renderer)
        {
            if (current is null) return; 
            DrawImage(renderer, current);
        }
    }
}
