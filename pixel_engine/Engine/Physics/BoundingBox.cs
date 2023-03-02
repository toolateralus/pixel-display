using System;
using System.Numerics;

namespace pixel_renderer
{

    public struct BoundingBox2DInt
    {
        public Vector2 min, max;
        public BoundingBox2DInt(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }
        public BoundingBox2DInt(BoundingBox2DInt existing)
        {
            this.min = existing.min;
            this.max = existing.max;
        }
        public void ExpandTo(Vector2 point)
        {
            min.X = Math.Min(point.X, min.X);
            min.Y = Math.Min(point.Y, min.Y);
            max.X = Math.Max(point.X + 1, max.X);
            max.Y = Math.Max(point.Y + 1, max.Y);
        }
    }

    public struct BoundingBox2D
    {
        public Vector2 min, max;
        public BoundingBox2D(float min_x, float min_y, float max_x, float max_y)
        {
            this.min = new Vector2(min_x, min_y);
            this.max = new Vector2(max_x, max_y);
        }
        public BoundingBox2D(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }
        public BoundingBox2D(BoundingBox2D existing)
        {
            this.min = existing.min;
            this.max = existing.max;
        }
        public void ExpandTo(Vector2 point)
        {
            min.X = Math.Min(point.X, min.X);
            min.Y = Math.Min(point.Y, min.Y);
            max.X = Math.Max(point.X + 1, max.X);
            max.Y = Math.Max(point.Y + 1, max.Y);
        }
    }
}


