using System;
using System.Runtime.CompilerServices;

namespace pixel_core
{
    public static class NumberExtensionMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WithinRange(this float v, float min, float max) { return v <= max && v >= min; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WithinRange(this int v, int min, int max) { return v <= max && v >= min; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(this double v, double min, double max) => Math.Min(max, Math.Max(v, min));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Squared(this double v) => v * v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Squared(this float v) => v * v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamped(this float v, float min, float max) => MathF.Min(max, MathF.Max(v, min));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this ref float v, float min, float max) => v = MathF.Min(max, MathF.Max(v, min));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Wrapped(this float v, float max)
        {
            float result = v - max * MathF.Floor(v / max);
            if (result >= max)
                return result - max;
            if (result < 0)
                return result + max;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Wrap(this ref float v, float max)
        {
            v -= max * MathF.Floor(v / max);
            if (v >= max)
                v -= max;
            else if (v < 0)
                v += max;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithin(this float v, float min, float max) => v >= min && v <= max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinMaxExclusive(this float v, float min, float max) => v >= min && v < max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDivideSafe(this float v) => v == 0 ? float.Epsilon : v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakeDivideSafe(this float[] v) { for (int i = 0; i < v.Length; i++) v[i] = v[i].GetDivideSafe(); }
    }
}