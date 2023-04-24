using System.ComponentModel;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using pixel_renderer.FileIO;

namespace pixel_renderer
{
    public class Animation : Animation<JImage>
    {
        [JsonProperty] internal bool playing;
        public Animation(Metadata[] frameData, int frameTime = 24)
        {
            if (frameData is null)
                return;
            this.frameTime = frameTime;
            for (int i = 0; i < frameData.Length * this.frameTime; i += this.frameTime)
            {
                Metadata? imgMetadata = frameData[i / this.frameTime];
                if (imgMetadata.extension != Constants.PngExt && imgMetadata.extension != Constants.BmpExt)
                    continue;
                Bitmap img = new(imgMetadata.Path);
                if (img is not null)
                {
                    var colors = CBit.PixelFromBitmap(img);
                    JImage image = new(colors);
                    SetValue(i, image);
                }
            }
        }
    }
}