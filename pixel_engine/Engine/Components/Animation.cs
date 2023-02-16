using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using pixel_renderer.FileIO;

namespace pixel_renderer
{
    public class Animation : Asset
    {

        [JsonProperty]
        internal bool playing;

        [JsonProperty]
        public bool looping = false;

        /// <summary>
        /// this is the distance between each frame, or number of frames a frame will display for.
        /// </summary>
        [JsonProperty]
        public int padding = 1;

        [JsonProperty]
        internal int startIndex = 0;

        internal int frameIndex = 0;

        [JsonProperty]
        public Dictionary<(int, int), Color[,]> frames = new();

        Color[,] lastFrame = new Color[,] {
                {
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                },
                {
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                    Color.Red, Color.White, Color.Blue,
                }
        };

        public Animation(Metadata[] frameData, int framePadding = 24) => CreateAnimation(frameData, framePadding);

        public void CreateAnimation(Metadata[] frameData, int framePadding = 24)
        {
            padding = framePadding;

            for (int i = 0; i < frameData.Length * padding; i += padding)
            {
                Metadata? imgMetadata = frameData[i / padding];

                if (imgMetadata.extension != Constants.BitmapFileExtension)
                    continue;

                Bitmap img = new(imgMetadata.fullPath);

                frames.Add((i, i + padding - 1), CBit.ColorArrayFromBitmap(img));
            }
        }
       


        public Color[,] GetFrame()
        {
            if (frameIndex > frames.Count * padding - 1 && looping)
                frameIndex = startIndex;

            foreach (var frame in frames)
                    if (frameIndex.WithinRange(frame.Key.Item1, frame.Key.Item2))
                        lastFrame = frame.Value;
            
            frameIndex++;

            return lastFrame;
        }
    }
}
