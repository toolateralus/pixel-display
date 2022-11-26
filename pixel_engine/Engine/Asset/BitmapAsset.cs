using Newtonsoft.Json;
using System.Drawing;

namespace pixel_renderer.Assets
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class BitmapAsset : Asset
    {
        public Bitmap RuntimeValue;
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