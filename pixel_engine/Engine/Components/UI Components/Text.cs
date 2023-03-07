﻿using System.Collections.Generic;
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
        public Curve posCurve;
        private JImage init_font;
        const string alphabet = "abcdefghijklmnopqrstuvwxyz"; 

        public override void Awake()
        {
            posCurve = Curve.Circlular(1, 16, 6, true);
            lock (font)
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
            init_font = JImage.Concat(font.Values, posCurve);
        }
        public override void Update()
        {


        }
        public override void Draw(RendererBase renderer)
        {
          
        }
    }
}
