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
        public static Vec2 one = new Vec2(1, 1);
        public static Vec2 zero = new Vec2(0, 0);
        internal static Vec2 up = new(0, -1);
        internal static Vec2 down = new(0, 1);
        internal static Vec2 left = new(-1, 0);
        internal static Vec2 right = new(1, 0);

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
    }
}


