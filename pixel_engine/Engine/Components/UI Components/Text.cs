using System.Collections.Generic;
using System.Drawing;
using pixel_renderer.Assets;
using System.Numerics;
using System.Linq;

namespace pixel_renderer
{
    public enum InterpolationType { Linear, Exponential, ExponentialSqrd}

    public class Text : Image
    {
        public Dictionary<char , JImage> font_model = new();
        /// <summary>
        /// the bounding box of the text element
        /// </summary>

        public Vector2 start = new(0, 0);
        public Vector2 end = new(1000, 0);

        public Curve posCurve;
        public Text() : base() { }
       

        public string Content
        {
            get => content;
            set
            {
                content = value;
                RefreshCharacters();
            }
        } 
        const string alphabet = "abcdefghijklmnopqrstuvwxyz ";
        [Field]
        private string content = "abc";
        public override void Awake()
        {
            base.Awake();
            Type = ImageType.Image;
            color = Pixel.White;
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

            if (content.Length == 0) 
                return; 
          
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
