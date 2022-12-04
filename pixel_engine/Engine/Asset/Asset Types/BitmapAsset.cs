using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace pixel_renderer.Assets
{
    public record BitmapAsset : Asset
    {
        [JsonIgnore]
        public Bitmap RuntimeValue;
        private Color[,] _colors; 
        public Color[,] Colors
        {
            get
            {
                unsafe
                {
                    if (_colors is null && RuntimeValue is not null) 
                        return ColorArrayFromBitmap();
                    else if (_colors is not null)  
                        return _colors;
                    throw new NullReferenceException("Couldnt get colors from bitmap");
                }
            }
            set =>  _colors = value; 
        }

        /// <summary>
        /// Clones the current value of the Bitmap stored within the Asset and caches it to a color array
        /// </summary>
        private unsafe Color[,] ColorArrayFromBitmap()
        {
            CBit.ReadonlyBitmapData(in RuntimeValue ,out BitmapData bmd, out int stride, out byte[] data);
            _colors = CBit.ColorArrayFromBitmapData(bmd, stride, data);
            return _colors;
        }

        public BitmapAsset(string name, Bitmap runtimeValue) : base(name, typeof(Bitmap))
        {
            RuntimeValue = runtimeValue;
            Name = name;
        }
        [JsonConstructor]
        public BitmapAsset(string Name, Color[,] Colors, string UUID, Bitmap runtimeValue) : base(Name, typeof(Bitmap), UUID)
        {
            _colors = Colors;
            this.RuntimeValue = runtimeValue; 
        }
        public static BitmapAsset PathToAsset(string filePath, string assetName)
        {
            Bitmap bmp = new(filePath);
            BitmapAsset asset = new(assetName, bmp);
            return asset;
        }
    }
}