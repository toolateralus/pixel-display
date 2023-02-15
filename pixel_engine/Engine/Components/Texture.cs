using System;
using System.Drawing;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using pixel_renderer.Scripts;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    public interface IAnimate
    {
        /// <summary>
        /// Starts the animation.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="looping"></param>
        public abstract void Start(float speed = 1, bool looping = true);
        /// <summary>
        /// Stops the animation.
        /// </summary>
        /// <param name="reset"></param>
        public abstract void Stop(bool reset = false);
        public abstract void Reset();

        /// <summary>
        /// Gets the next frame in the animation, or skips frames if  an increment of greater than one is provided
        /// /// </summary>
        /// <param name="increment"></param>
        public virtual void Next(int increment = 1)
        {

        }

        /// <summary>
        /// Gets the previous frame in the animation, or skips back multiple frames if an increment of greater than one is provided
        /// </summary>
        /// <param name="increment"></param>
        public virtual void Previous(int increment = 1)
        {

        }
    }

    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(Metadata imgData, Metadata maskData, Color? color, string Name, string UUID) : base(Name, UUID)
        {
            this.imgData = imgData;
            this.maskData = maskData;
            this.color = color;
            this.Name = Name;
            Upload();
        }
        public Texture(Vec2Int scale, Metadata? imgData = null, Color? color = null)
        {
            this.scale = scale; 
            this.color = color;

            if (imgData is not null)
            {
                this.imgData = imgData;
                Image = new(imgData.fullPath);
            }
            else imgData = Player.test_image_data;

            if(color is not null)  Image = CBit.SolidColorBitmap(this.scale, (Color)color);
            
            
        }
         public Bitmap? Image { get; set; }

        [Field] public Bitmap? Mask;

        [JsonProperty] internal Metadata imgData;
        [JsonProperty] internal Metadata maskData;

        [Field] [JsonProperty] public Color? color;
        [Field] [JsonProperty] public Vec2Int scale = new(1, 1);
        
        public bool HasImage => Image != null;
        internal bool HasImageMetadata => imgData != null;

        public bool HasMask => Mask != null;
        internal bool HasMaskMetadata => imgData != null;

        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);
    }
}
