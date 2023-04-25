using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;
using System.Linq;
using System.Windows.Media;

namespace pixel_renderer
{
    public enum InterpolationType { Linear, Exponential, ExponentialSqrd}

    public class Text : Image
    { 
        public static ICollection<System.Windows.Media.FontFamily> GetSystemFonts()
        {
            return Fonts.SystemFontFamilies;
        }
        public Dictionary<char , JImage> font_model = new();
        /// <summary>
        /// the bounding box of the text element
        /// </summary>
        public Curve posCurve;
        const string alphabet = "abcdefghijklmnopqrstuvwxyz ";
        public string Content
        {
            get => content;
            set
            {
                content = value;
                RefreshCharacters();
            }
        } 
        [Field]
        private string content = "abc";
        public override void Awake()
        {
            base.Awake();

            Type = ImageType.Image;
            color = Pixel.Red;

            RefreshFont();
            RefreshCharacters();
            Refresh(); 
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
            if (content.Length == 0) 
                return; 
          
            var output = new List<JImage>();
            int width = 0;
            int height = 0;

            for (int i = 0; i < Content.Length; i++)
                if (font_model.ContainsKey(Content[i]))
                {
                    var img = font_model.ElementAt(i).Value;

                    output.Add(img.Clone());

                    width += img.width;
                    height += img.height; 
                }

            var start = new Vector2(0, 0);
            var end = new Vector2(0, width);

            posCurve = Curve.Linear(start, end, 1, width / Content.Length);
            var concatenatedImg = JImage.Concat(output, posCurve);

            if (texture is null) texture = new(concatenatedImg);
            else texture.SetImage(concatenatedImg);

            Scale = new(width, height);
        }
        public override void Draw(RendererBase renderer)
        {
            if (texture != null)
            {
                var texture = this.texture.GetImage();
                DrawImage(renderer, texture);
            }
        }
       
    }
}
