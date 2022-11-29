using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
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
      
        /// <summary>
        /// clones the current Bitmap stored in the Asset object and returns out BitmapData from that copy.
        /// </summary>
        /// <param name="bmd"></param>
        /// <param name="stride"></param>
        /// <param name="data"></param>
       

        public BitmapAsset(string name, Bitmap runtimeValue) : base(name, typeof(Bitmap))
        {
            RuntimeValue = runtimeValue;
            Name = name;
        }
        [JsonConstructor]
        public BitmapAsset(string Name, Color[,] Colors, string UUID) : base(Name, typeof(Bitmap), UUID)
        {
            _colors = Colors;
        }
        public static BitmapAsset PathToAsset(string filePath, string assetName)
        {
            Bitmap bmp = new(filePath);
            BitmapAsset asset = new(assetName, bmp);
            return asset;
        }
    }
}