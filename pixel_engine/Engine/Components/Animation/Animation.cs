﻿using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// this is the number of frames to wait between displaying frames.
        /// </summary>
        [JsonProperty]
        public int padding = 1;

        [JsonProperty]
        internal int startIndex = 0;

        internal int frameIndex = 0;

        [JsonProperty]
        public Dictionary<(int, int), JImage> frames = new();

        JImage? lastFrame;

        [Field] [JsonProperty]
        public float speed = 1.0f; 

        public Animation(Metadata[] frameData, int framePadding = 24) => CreateAnimation(frameData, framePadding);

        public void CreateAnimation(Metadata[] frameData, int framePadding = 24)
        {

            if (frameData is null)
                return;

            padding = framePadding;

            for (int i = 0; i < frameData.Length * padding; i += padding)
            {
                Metadata? imgMetadata = frameData[i / padding];

                if (imgMetadata.extension != Constants.PngExt && imgMetadata.extension != Constants.BmpExt)
                    continue;

                Bitmap img = new(imgMetadata.Path);

                if (img is not null)
                {
                    var colors = CBit.PixelFromBitmap(img);
                    JImage image = new(colors);

                    frames.Add((i, i + padding - 1), image);
                }


            }
        }

        public JImage GetFrame(bool shouldIncrement = true)
        {
            if (frameIndex > frames.Count * padding - 1 && looping)
                frameIndex = startIndex;

            if (shouldIncrement)
                frameIndex = (int)(frameIndex + speed); // use the speed field to control the frame rate

            foreach (var frame in frames)
                if (frameIndex.WithinRange(frame.Key.Item1, frame.Key.Item2))
                    lastFrame = frame.Value;

            return lastFrame;
        }
    }
}