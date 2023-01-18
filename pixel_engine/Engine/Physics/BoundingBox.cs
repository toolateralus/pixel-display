namespace pixel_renderer
{
    using System;

    public struct BoundingBox2DInt
    {
        public Vec2Int min, max;
        public BoundingBox2DInt(Vec2Int min, Vec2Int max)
        {
            this.min = min;
            this.max = max;
        }
        public BoundingBox2DInt(BoundingBox2DInt existing)
        {
            this.min = existing.min;
            this.max = existing.max;
        }
        public void ExpandTo(Vec2Int point)
        {
            min.x = Math.Min(point.x, min.x);
            min.y = Math.Min(point.y, min.y);
            max.x = Math.Max(point.x + 1, max.x);
            max.y = Math.Max(point.y + 1, max.y);
        }
    }

    public struct BoundingBox2D
    {
        public Vec2 min, max;
        public BoundingBox2D(Vec2 min, Vec2 max)
        {
            this.min = min;
            this.max = max;
        }
        public BoundingBox2D(BoundingBox2D existing)
        {
            this.min = existing.min;
            this.max = existing.max;
        }
        public void ExpandTo(Vec2 point)
        {
            min.x = Math.Min(point.x, min.x);
            min.y = Math.Min(point.y, min.y);
            max.x = Math.Max(point.x + 1, max.x);
            max.y = Math.Max(point.y + 1, max.y);
        }
    }
}


