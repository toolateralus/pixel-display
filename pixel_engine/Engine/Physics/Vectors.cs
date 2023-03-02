namespace pixel_renderer
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text.Json.Serialization;
    using System.Windows;
    //public struct Vector3
    //{
    //    public float x;
    //    public float y;
    //    public float z;
    //    public float Magnitude() => MathF.Sqrt(x * x + y * y + z * z);
    //    public float SqrMagnitude() => x * x + y * y + z * z;
    //    public float this[int index]
    //    {
    //        get =>
    //            index switch
    //            {
    //                0 => x,
    //                1 => y,
    //                2 => z,
    //                _ => throw new IndexOutOfRangeException(),
    //            };
    //        set
    //        {
    //            switch (index)
    //            {
    //                case 0: x = value; break;
    //                case 1: y = value; break;
    //                case 2: z = value; break;
    //                default: throw new IndexOutOfRangeException();
    //            }
    //        }

    //    }
    //    public readonly static Vector3 one = new(1, 1, 1);
    //    public readonly static Vector3 zero = new(0, 0, 0);

    //    public Vector3()
    //    {
    //        x = 0;
    //        y = 0;
    //        z = 0;
    //    }
    //    public Vector3(float x, float y, float z)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //        this.z = z;
    //    }

    //    public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.X + b.X a.Y + b.Y, a.z + b.z); }
    //    public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.X - b.X a.Y - b.Y, a.z - b.z); }
    //    public static Vector3 operator /(Vector3 a, Vector3 b) { return new Vector3(a.X / b.X a.Y / b.Y, a.z / b.z); }
    //    public static Vector3 operator *(Vector3 a, Vector3 b) { return new Vector3(a.X * b.X a.Y * b.Y, a.z * b.z); }

    //    public static implicit operator Vector2(Vector3 v) => new()
    //    {
    //        x = v.X
    //        y = v.Y
    //    };
    //    public static implicit operator Vector3(Vector2 v) => new()
    //    {
    //        x = v.X
    //        y = v.Y,
    //        z = 0
    //    };

    //}
    //[JsonObject(MemberSerialization.OptIn)]
    //public struct Vector2
    //{
    //    [JsonProperty]
    //    public float x;

    //    [JsonProperty]
    //    public float y;
    //    public Vector2 Normal_RHS => new Vector2(-y, x).Normalized();
    //    public Vector2 Normal_LHS() => new Vector2(y, -x).Normalized();

    //    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X a.Y + b.Y);
    //    public static bool operator ==(Vector2 a, Vector2 b) => a.X == b.X && a.Y == b.Y;
    //    public static bool operator !=(Vector2 a, Vector2 b) => a.X != b.X || a.Y != b.Y;
    //    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X a.Y - b.Y);
    //    public static Vector2 operator /(Vector2 a, float b) => new(a.X / b, a.Y / b);
    //    public static Vector2 operator *(Vector2 a, float b) => new(a.X * b, a.Y * b);
    //    public static Vector2 operator *(Vector2 a, Vector2 b) => new()
    //    {
    //        x = a.X * b.X
    //        y = a.Y * b.Y,
    //    };
    //    public static Vector2 operator /(Vector2 a, Vector2 b) => new()
    //    {
    //        x = a.X / b.X
    //        y = a.Y / b.Y,
    //    };
    //    public static implicit operator Point(Vector2 v) => new()
    //    {
    //        X = v.X
    //        Y = v.Y
    //    };
    //    public static explicit operator Vector2(Point v) => new((float)v.X, (float)v.Y);
    //    /// <summary>
    //    /// Clamp each value of the vector component-wise;
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <param name="min"></param>
    //    /// <param name="max"></param>


    //    public void Set(Vector2 size) => this = size; 

    //    public override bool Equals(object? obj)
    //    {
    //        if (obj is not Vector2 vec)
    //            return false;
    //        return x.Equals(vec.x) && y.Equals(vec.Y);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return x.GetHashCode() ^ y.GetHashCode();
    //    }

    //    public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
    //    {
    //        if (vector.SqrMagnitude() > maxLength * maxLength)
    //        {
    //            return vector.Normalized() * maxLength;
    //        }
    //        return vector;
    //    }
    //}
    //public struct Vector2
    //{
    //    public int x;
    //    public int y;

    //    public void Increment2D(int xMax, int xMin = 0)
    //    {
    //        x++;
    //        if (x >= xMax)
    //        {
    //            y++;
    //            x = xMin;
    //        }
    //    }
    //    public Vector2(int x, int y)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //    }
    //    public Vector2(Vector2 v)
    //    {
    //        this.X = v.x;
    //        this.Y = v.Y;
    //    }
    //    public Vector2(Vector2 v)
    //    {
    //        this.X = (int)v.x;
    //        this.Y = (int)v.Y;
    //    }
    //    public int this[int index]
    //    {
    //        get
    //        {
    //            return index switch
    //            {
    //                0 => x,
    //                1 => y,
    //                _ => 0,
    //            };
    //        }
    //        set
    //        {
    //            switch (index)
    //            {
    //                case 0: x = value; break;
    //                case 1: y = value; break;
    //            }
    //        }

    //    }
    //    public static implicit operator Vector2(Vector2 v) => new(v.X v.Y);
    //    public static explicit operator Vector2(Vector2 v) => new((int)v.X (int)v.Y);
    //    public static Vector2 operator +(Vector2 v1, Vector2 v2) => new(v1.X + v2.X v1.Y + v2.Y);
    //    public static Vector2 operator -(Vector2 v1, Vector2 v2) => new(v1.X - v2.X v1.Y - v2.Y);
    //}
}


