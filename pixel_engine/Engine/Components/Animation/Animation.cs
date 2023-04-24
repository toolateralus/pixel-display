using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using pixel_renderer.FileIO;

namespace pixel_renderer
{
    public class Tween<T>
    {
        [JsonProperty] internal bool playing;
        [JsonProperty] public bool looping = false;
        /// <summary>
        /// this is the number of frames to wait between displaying frames.
        /// </summary>
        [JsonProperty] public int frameTime = 1;
        [JsonProperty] internal int startIndex = 0;
        [JsonProperty] public Dictionary<(int, int), T> frames = new();

        [Field]
        [JsonProperty]
        public float speed = 1.0f;

        internal int frameIndex = 0;
        T? lastFrame;
        public Tween(T[] frameData, int frameTime = 24)
        {
            if (frameData is null)
                return;

            this.frameTime = frameTime;

            for (int i = 0; i < frameData.Length * this.frameTime; i += this.frameTime)
            {
                T? value = frameData[i / this.frameTime];
                SetValue(i, value);
            }
        }
        public void SetValue(int index, T value)
        {
            frames.Add((index, index + frameTime - 1), value);
        }
        public T GetValue(bool shouldIncrement = true)
        {
            if (frameIndex > frames.Count * frameTime - 1 && looping)
                frameIndex = startIndex;

            if (shouldIncrement) Increment();

            foreach (var frame in frames)
                if (frameIndex.WithinRange(frame.Key.Item1, frame.Key.Item2))
                    lastFrame = frame.Value;

            if (lastFrame is null)
                throw new NullReferenceException();

            return lastFrame;
        }
        public void Increment()
        {
            frameIndex = (int)(frameIndex + speed);
        }
        public void Decrement()
        {
            frameIndex = (int)(frameIndex - speed);
        }
    }

    public class Animation : Asset
    {
        [JsonProperty] internal bool playing;
        [JsonProperty] public bool looping = false;
        /// <summary>
        /// this is the number of frames to wait between displaying frames.
        /// </summary>
        [JsonProperty] public int frameTime = 1;
        [JsonProperty] internal int startIndex = 0;
        [JsonProperty] public Dictionary<(int, int), JImage> frames = new();
       
        [Field]
        [JsonProperty]
        public float speed = 1.0f;

        internal int frameIndex = 0;
        JImage? lastFrame;
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
                    InsertFrame(i, image);
                }
            }
        }

        private void InsertFrame(int index, JImage image)
        {
            frames.Add((index, index + frameTime - 1), image);
        }

        public JImage GetFrame(bool shouldIncrement = true)
        {
            if (frameIndex > frames.Count * frameTime - 1 && looping)
                frameIndex = startIndex;

            if (shouldIncrement) Increment();

            foreach (var frame in frames)
                if (frameIndex.WithinRange(frame.Key.Item1, frame.Key.Item2))
                    lastFrame = frame.Value;

            if (lastFrame is null)
                throw new NullReferenceException(); 

            return lastFrame;
        }

        private void Increment()
        {
            frameIndex = (int)(frameIndex + speed);
        }
    }
}