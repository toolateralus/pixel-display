namespace pixel_renderer
{
    using System;
    using pixel_renderer;

    public static class WaveForms
    {
        public static int vertices = 1024;
        public static float amplitude = 1;
        public static float frequency = .5f;
        public static Vec2 xLimits = new Vec2(0, 15);
        public static float movementSpeed = .5f;
        public static float radians = 2 * CMath.PI;
        /// <summary>
        /// Samples a random vertex point on a Sine Wave operating within pre-defined parameters.
        /// </summary>
        public static Vec2 Next => GetPointOnSine();
        /// <summary>
        /// Manually define parameters for a sample from a sine wave.
        /// </summary>
        /// <param name="startPosition">the start of the wave</param>
        /// <param name="endPosition">the end position of the wave</param>
        /// <param name="Tau">A float within the range of 0 to PI * 2</param>
        /// <param name="vertexIndex">the individual vertex of the wave which will be returned</param>
        /// <param name="x">out X of the returned vector</param>
        /// <param name="y">out Y of the returned vector</param>
        /// <returns>A Vertex position on the specified wave.</returns>
        public static Vec2 GetPointOnSine(float startPosition = 0, float endPosition = 1, float Tau = CMath.PI * 2, int vertexIndex = 0)
        {
            float progress = (float)vertexIndex / (vertices - 1);
            int x = (int)CMath.Lerp(startPosition, endPosition, progress);
            int y = (int)(amplitude * Math.Sin(Tau * frequency * x + Runtime.Instance.frameCount * movementSpeed));
            return new Vec2(x, y);
        }
        /// <summary>
        /// Sample a sine wave under the current defined parameters of the static class Sine.
        /// </summary>
        /// <returns>A Vertex position at a random point on a sine wave</returns>
        public static Vec2 GetPointOnSine()
        {
            int vertexIndex = JRandom.Int(0, vertices);
            const float Tau = CMath.PI * 2;
            float progress = (float)vertexIndex / (vertices - 1);
            int x = (int)CMath.Lerp(0, 1, progress);
            int y = (int)(amplitude * Math.Sin(Tau * frequency * x + Runtime.Instance.frameCount * movementSpeed));
            return new Vec2(x, y);
        }
    }

}


