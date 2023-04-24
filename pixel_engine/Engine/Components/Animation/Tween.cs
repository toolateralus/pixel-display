using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace pixel_renderer
{
    public class Animation<T>
    {
        [JsonProperty] public bool looping = true;
        [JsonProperty] public bool reversed = true;
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
        public T? lastFrame;
        /// <summary>
        /// frameTime is exclusive (I think, if you input 16 frames will be 15 cycles long.)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="frameTime"></param>
        /// <param name="looping"></param>
        public Animation(T[] data = null, int frameTime = 24, bool looping = true, bool reversed = false)
        {
            this.reversed = reversed;
            this.looping = looping;
            this.frameTime = frameTime;
            if (data is null)
            {
                Runtime.Log($"Framedata was null in tween of type {typeof(T)} this could be intended.");
                return;
            }
            Refresh(data);
        }
        public virtual void Refresh(T[] frameData)
        {
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

            if (frameIndex < 0)
                frameIndex = frames.Count * frameTime - 1; 

            if (shouldIncrement)
            {
                if (!reversed)
                    Increment();
                else Decrement();
            }

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
   
}