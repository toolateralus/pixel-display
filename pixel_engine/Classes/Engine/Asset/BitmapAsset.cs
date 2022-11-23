using System.Drawing;

namespace pixel_renderer.Assets
{
    public class BitmapAsset : Asset
    {
        public Bitmap? RuntimeValue = null;
        public BitmapAsset(string name) : base(name, typeof(Bitmap))
        {
            Name = name;
            fileType = typeof(Bitmap);
        }
        public static BitmapAsset BitmapToAsset(string fileName, string assetName)
        {
            Bitmap? bmp = new(fileName);
            BitmapAsset asset = new(assetName);
            if (bmp != null)
                asset.RuntimeValue = bmp;
            return asset;
        }
    }
}