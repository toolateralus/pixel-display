using System.Numerics;

namespace Pixel.Types.Physics
{
    /// <summary>
    ///  A single SAT projection.
    /// </summary>
    public class SATProjection
    {
        public float min;
        public float max;
    }
    /// <summary>
    /// A single SAT contact point, resulting from a projection.
    /// </summary>
    public class SATContanctPoint
    {
        public Vector2 point;
        public Vector2 normal;
    }

}