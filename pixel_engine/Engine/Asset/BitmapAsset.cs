using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace pixel_renderer.Assets
{
    public class BitmapAsset : Asset
    {
        [JsonIgnore]
        public Bitmap RuntimeValue;

        public List<List<Color>> Colors 
        { 
            get 
            {
                List<List<Color>> colors = new();
                for (int i = 0; i< RuntimeValue.Width; i++)
                {
                    List <Color> color = new List<Color>();
                    for (int j = 0; j < RuntimeValue.Height; j++)
                            color.Add(RuntimeValue.GetPixel(i, j));
                    colors.Add(color); 
                }

                return colors; 
            }
        }

        public BitmapAsset(string name) : base(name, typeof(Bitmap))
        {
            Name = name;
        }
        public static BitmapAsset BitmapToAsset(string fileName, string assetName)
        {
            Bitmap bmp = new(fileName);
            BitmapAsset asset = new(assetName);
            if (bmp != null)
                asset.RuntimeValue = bmp;
            return asset;
        }
    }
}