using pixel_editor;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace pixel_renderer
{
    public static class Vector2ExtensionMethods
    {
        public readonly static Vector2 one = new(1, 1);
        public readonly static Vector2 zero = new(0, 0);
        public readonly static Vector2 up = new(0, -1);
        public readonly static Vector2 down = new(0, 1);
        public readonly static Vector2 left = new(-1, 0);
        public readonly static Vector2 right = new(1, 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normal_LHS(this Vector2 v)
        {
            float x = v.X;
            v.X = v.Y;
            v.Y = -x;
            v.Normalize();
            return v;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSquared(Vector2 a, Vector2 b)
        {
            return (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SqrDistanceFrom(this Vector2 a, Vector2 v)
        {
            return DistanceSquared(a, v);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceFrom(this Vector2 v, Vector2 a)
        {
            return v.Distance(a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this Vector2 a, Vector2 b)
        {
            var distanceSquared = DistanceSquared(a, b);
            return CMath.Sqrt(distanceSquared);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(this Vector2 a, Vector2 b)
        {
            return (a.X * b.X) + (a.Y * b.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Length(this Vector2 v) => MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Rotated(this Vector2 v, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector2(cos * v.X - sin * v.Y, sin * v.X + cos * v.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rotate(this ref Vector2 v, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float newX = cos * v.X - sin * v.Y;
            float newY = sin * v.X + cos * v.Y;
            v.X = newX;
            v.Y = newY;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SqrMagnitude(this Vector2 v)
        {
            var product = MathF.FusedMultiplyAdd(v.X, v.X, v.Y * v.Y);
            return product;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Wrap(this Vector2 v, Vector2 max) { v.X = v.X.Wrapped(max.X); v.Y = v.Y.Wrapped(max.Y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Wrapped(this Vector2 v, Vector2 max) => new(v.X.Wrapped(max.X), v.Y.Wrapped(max.Y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithin(this Vector2 v, Vector2 min, Vector2 max) => v.X.IsWithin(min.X, max.X) && v.Y.IsWithin(min.Y, max.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinMaxExclusive(this Vector2 v, Vector2 min, Vector2 max) => v.X.IsWithinMaxExclusive(min.X, max.X) && v.Y.IsWithinMaxExclusive(min.Y, max.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transformed(this Vector2 v, Matrix3x2 matrix) => Vector2.Transform(v, matrix);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this System.Windows.Point v) => new((float)v.X, (float)v.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(this ref Vector2 v, Matrix3x2 matrix) => v = Vector2.Transform(v, matrix);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakeDivideSafe(this ref Vector2 v)
        {
            v.X = v.X.GetDivideSafe();
            v.Y = v.Y.GetDivideSafe();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetDivideSafe(this Vector2 v)
        {
            v.X = v.X.GetDivideSafe();
            v.Y = v.Y.GetDivideSafe();
            return v;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithValue(this Vector2 v, int? x = null, int? y = null) { return new Vector2(x ?? v.X, y ?? v.Y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithValue(this Vector2 v, float? x = null, float? y = null) { return new Vector2(x ?? v.X, y ?? v.Y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithScale(this Vector2 v, float x = 1, float y = 1) { return new Vector2(v.X * x, v.Y * y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum(this Vector2 v) => v.X + v.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(this ref Vector2 v, Vector2 min, Vector2 max)
        {
            v.X = v.X.Clamped(min.X, max.X);
            v.Y = v.Y.Clamped(min.Y, max.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum(this Vector3 v) => v.X + v.Y + v.Z;
        /// <summary>
        ///  TODO: fix possible  'divide by zero'
        ///   Normalize a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A normalized Vector from the length of the current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalized(this Vector2 v)
        {
            if (v.Equals(Vector2.Zero))
                return Vector2.Zero;
            return v / v.Length();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(this ref Vector2 v)
        {
            if (v.Equals(Vector2.Zero))
                v = Vector2.Zero;
            v = v / v.Length();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Increment2D(this ref Vector2 v, float xMax, float xMin = 0)
        {
            v.X++;
            if (v.X >= xMax)
            {
                v.Y++;
                v.X = xMin;
            }
        }
    }
}