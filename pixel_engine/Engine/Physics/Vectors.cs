namespace pixel_renderer
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text.Json.Serialization;
    using System.Windows;
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;
        public float Magnitude() => MathF.Sqrt(x * x + y * y + z * z);
        public float SqrMagnitude() => x * x + y * y + z * z;
        public float this[int index]
        {
            get =>
                index switch
                {
                    0 => x,
                    1 => y,
                    2 => z,
                    _ => throw new IndexOutOfRangeException(),
                };
            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }

        }
        public static Vec3 one = new Vec3(1, 1, 1);
        public static Vec3 zero = new Vec3(0, 0, 0);

        public Vec3()
        {
            x = 0;
            y = 0;
            z = 0;
        }
        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vec3 operator /(Vec3 a, Vec3 b) { return new Vec3(a.x / b.x, a.y / b.y, a.z / b.z); }
        public static Vec3 operator *(Vec3 a, Vec3 b) { return new Vec3(a.x * b.x, a.y * b.y, a.z * b.z); }

        public static implicit operator Vec2(Vec3 v) => new()
        {
            x = v.x,
            y = v.y
        };
        public static implicit operator Vec3(Vec2 v) => new()
        {
            x = v.x,
            y = v.y,
            z = 0
        };

    }
    public struct Vec2
    {
        public float x;
        public float y;
        public Vec2 Normal_RHS => new Vec2(-y, x).Normalize();
        public Vec2 Normal_LHS => new Vec2(y, -x).Normalize();
        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => x,
                    1 => y,
                    _ => throw new IndexOutOfRangeException(),
                };
            }
            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
        public static float Dot(Vec2 a, Vec2 b)
        {
            return (a.x * b.x) - (a.y * b.y);
        }
        public float Length() => (float)Math.Sqrt(x * x + y * y);
        public Vec2 Rotated(float angle)
        {
            float xResult = (MathF.Cos(angle) * x) - (MathF.Sin(angle) * y);
            float yResult = (MathF.Sin(angle) * x) + (MathF.Cos(angle) * y);
            return new Vec2(xResult, yResult);
        }
        public void Rotate(float angle)
        {
            x = (MathF.Cos(angle) * x) - (MathF.Sin(angle) * y);
            y = (MathF.Sin(angle) * x) + (MathF.Cos(angle) * y);
        }
        public float SqrMagnitude() => x * x + y * y;
        public static Vec2 one = new(1, 1);
        public static Vec2 zero = new(0, 0);
        internal static Vec2 up = new(0, -1);
        internal static Vec2 down = new(0, 1);
        internal static Vec2 left = new(-1, 0);
        internal static Vec2 right = new(1, 0);

        public Vec2(Vec2 original)
        {
            x = original.x;
            y = original.y;
        }
        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public Vec2()
        {
            x = new();
            y = new();
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.x + b.x, a.y + b.y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.x - b.x, a.y - b.y);
        public static Vec2 operator /(Vec2 a, float b) => new(a.x / b, a.y / b);
        public static Vec2 operator *(Vec2 a, float b) => new(a.x * b, a.y * b);
        public static Vec2 operator *(Vec2 a, Vec2 b) => new()
        {
            x = a.x * b.x,
            y = a.y * b.y,
        };
        public static Vec2 operator /(Vec2 a, Vec2 b) => new()
        {
            x = a.x / b.x,
            y = a.y / b.y,
        };
        public static implicit operator Point(Vec2 v) => new()
        {
            X = v.x,
            Y = v.y
        };
        public static explicit operator Vec2(Point v) => new((float)v.X, (float)v.Y);
        /// <summary>
        /// Clamp each value of the vector component-wise;
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        internal void Clamp(Vec2 min, Vec2 max)
        {
            x = x.Clamp(min.x, max.x);
            y = y.Clamp(min.y, max.y);
        }
        internal static void Clamp(ref Vec2 value, Vec2 min, Vec2 max)
        {
            value.x = value.x.Clamp(min.x, max.x);
            value.y = value.y.Clamp(min.y, max.y);
        }
        internal Vec2 Clamped(Vec2 min, Vec2 max) => new(x.Clamp(min.x, max.x), y.Clamp(min.y, max.y));
        
        internal void Wrap(Vec2 max) { x = x.Wrapped(max.x); y = y.Wrapped(max.y); }
        internal Vec2 Wrapped(Vec2 max) => new(x.Wrapped(max.x), y.Wrapped(max.y));
        
        internal bool IsWithin(Vec2 min, Vec2 max) => x.IsWithin(min.x, max.x) && y.IsWithin(min.y, max.y);
        internal bool IsWithinMaxExclusive(Vec2 min, Vec2 max) => x.IsWithinMaxExclusive(min.x, max.x) && y.IsWithinMaxExclusive(min.y, max.y);

        internal void Set(Vec2 size) => this = size; 
        internal void Set(int axis, float value) => this[axis] = value;
    
    }
    public struct Vec2Int
    {
        public int x;
        public int y;

        public void Increment2D(int xMax)
        {
            x++;
            if (x >= xMax)
            {
                y++;
                x = 0;
            }
        }
        public Vec2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Vec2Int(Vec2Int v)
        {
            this.x = v.x;
            this.y = v.y;
        }
        public Vec2Int(Vec2 v)
        {
            this.x = (int)v.x;
            this.y = (int)v.y;
        }
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                }
                return 0; 
            }
            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                }
            }

        }
        public static implicit operator Vec2(Vec2Int v) => new(v.x, v.y);
        public static explicit operator Vec2Int(Vec2 v) => new((int)v.x, (int)v.y);
    }
}


