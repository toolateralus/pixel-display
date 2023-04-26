using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace pixel_core
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
        public float X
        {
            get { return min.X; }
            set { min.X = value; }
        }

        public float Y
        {
            get { return min.Y; }
            set { min.Y = value; }
        }
       
        public float Width
        {
            get { return max.X - min.X; }
            set { max.X = min.X + value; }
        }

        public float Height
        {
            get { return max.Y - min.Y; }
            set { max.Y = min.Y + value; }
        }
        public bool Contains(Vector2 point)
        {
            return (point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y);
        }
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
        public BoundingBox2D(Vector2[] expandToAll)
        {
            this.min = expandToAll[0];
            this.max = expandToAll[0];
            for(int i = 1; i < expandToAll.Length; i++)
                ExpandTo(expandToAll[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExpandTo(Vector2 point)
        {
            min.X = Math.Min(point.X, min.X);
            min.Y = Math.Min(point.Y, min.Y);
            max.X = Math.Max(point.X, max.X);
            max.Y = Math.Max(point.Y, max.Y);
        }
        public bool Intersects(BoundingBox2D other)
        {
            return (max.X >= other.min.X) && (min.X <= other.max.X) &&
                   (max.Y >= other.min.Y) && (min.Y <= other.max.Y);
        }
    }
}


