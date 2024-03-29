﻿using Newtonsoft.Json;
using Pixel.FileIO;
using Pixel.Statics;
using System.Drawing;

namespace Pixel
{
    public class Animation : Animation<JImage>
    {
        [JsonProperty] public bool playing;
        public Animation(Metadata[] frameData, int frameTime = 24)
        {
            if (frameData is null)
                return;
            this.frameTime = frameTime;
            for (int i = 0; i < frameData.Length * this.frameTime; i += this.frameTime)
            {
                Metadata? imgMetadata = frameData[i / this.frameTime];
                if (imgMetadata.Extension != Constants.PngExt && imgMetadata.Extension != Constants.BmpExt)
                    continue;
                Bitmap img = new(imgMetadata.FullPath);
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