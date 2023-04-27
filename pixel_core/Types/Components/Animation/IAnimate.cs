namespace Pixel
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

        /// <summary>
        /// Gets the next frame in the animation, or skips frames if  an increment of greater than one is provided
        /// /// </summary>
        /// <param name="increment"></param>
        public abstract void Next();


        /// <summary>
        /// Gets the previous frame in the animation, or skips back multiple frames if an increment of greater than one is provided
        /// </summary>
        /// <param name="increment"></param>
        public abstract void Previous();

    }
}
