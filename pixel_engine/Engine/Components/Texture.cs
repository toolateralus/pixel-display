﻿using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(Metadata imgData, Vec2Int scale, string Name = "Texture Asset") : base(Name, true)
        {
            this.imgData = imgData;
            this.Name = Name;
            this.scale = scale; 
            SetImage(imgData, scale);
        }

        public void SetImage(Metadata imgData, Vec2Int scale)
        {
            this.scale = scale;

            if (imgData is not null)
            {
                this.imgData = imgData;
                Image = new(imgData.fullPath);
            }
            else
            {
                this.imgData = Player.PlayerSprite;
                Image = new(imgData.fullPath);
            }
        }

        public Texture(Vec2Int scale, Metadata imgData)
        {
            SetImage(imgData, scale);
        }

        [Field] [JsonProperty] public Vec2Int scale = new(1, 1);
        [JsonProperty] internal Metadata imgData;
        public Bitmap? Image { get; set; }
        public bool HasImage => Image != null;
        internal bool HasImageMetadata => imgData != null;
        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);
    }
}
