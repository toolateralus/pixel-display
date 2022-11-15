namespace pixel_renderer
{
    using System;
    using System.Windows;
    public class Vec3
    {
        public float x;
        public float y;
        public float z;
        public float Magnitude => MathF.Sqrt(x * x + y * y + z * z);
        public float sqrMagnitude => x * x + y * y + z * z;

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
    public class Vec2
    {
        public float x;
        public float y;
        public float Length => (float)Math.Sqrt(x * x + y * y);
        public float sqrMagnitude => x * x + y * y;
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
        { }

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
    }
    public enum RenderState { Game, Scene, Off}
}


