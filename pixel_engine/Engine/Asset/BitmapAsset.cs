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
        private Color[,] _colors; 
        public Color[,] Colors 
        { 
            get 
            {
                if (_colors is null && RuntimeValue is not null)
                {
                     _colors = new Color[RuntimeValue.Width, RuntimeValue.Height];
                    for (int i = 0; i < _colors.GetLength(0); i++)
                    {
                        for (int j = 0; j < _colors.GetLength(1); j++)
                            _colors[i, j] = RuntimeValue.GetPixel(i, j);
                    }
                    return _colors;
                }   
                else if (_colors is not null)
                return _colors;
                throw new NullReferenceException("Couldnt get colors from bitmap");
            }
            set =>  _colors = value; 
              
        }
        public BitmapAsset(string name, Bitmap runtimeValue) : base(name, typeof(Bitmap))
        {
            this.RuntimeValue = runtimeValue;
            Name = name;
        }
        [JsonConstructor]
        public BitmapAsset(string Name, Color[,] Colors, string UUID) : base(Name, typeof(Bitmap), UUID)
        {
            _colors = Colors;
        }
        public static BitmapAsset BitmapToAsset(string fileName, string assetName)
        {
            Bitmap bmp = new(fileName);
            BitmapAsset asset = new(assetName, bmp);
            return asset;
        }
    }
}