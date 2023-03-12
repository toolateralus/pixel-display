using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;
using System.Security.Policy;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection.Metadata.Ecma335;

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
        public Vector2 end = new(1000, 0);

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
        const string alphabet = "abcdefghijklmnopqrstuvwxyz";
        [Field]
        private string content = "abc";
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
                        
                        if (font_model.ContainsKey(alphabet[i]))
                            continue;

                        font_model.Add(alphabet[i], image);
                    }
                }
        }
        [Method]
        public void RefreshCharacters()
        {
            var output = new List<JImage>();
            int width = 0;
            int height = 0;

            for (int i = 0; i < Content.Length; i++)
                if (font_model.ContainsKey(Content[i]))
                {
                    var img = font_model.ElementAt(i).Value;
                    output.Add(img);

                    width += img.width;
                    height += img.height; 
                }

            var start = new Vector2(0, 0);
            var end = new Vector2(width, height);

            posCurve = Curve.Linear(start, end, 1, Content.Length);
            current = JImage.Concat(output, posCurve);

            Scale = new(100, 100);
            viewportPosition = new(0, 0);

        }
        public override void Draw(RendererBase renderer)
        {
            if (current is null)
                return;

            DrawImage(renderer, current);
        }

        public static (Node, Text) Standard(string name = "New Text")
        {
            Node node = new("Text");
            var text = node.AddComponent<Text>();
            return (node, text);
        }
    }
}
