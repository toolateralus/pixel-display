using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace pixel_renderer
{
    public static class ExtensionMethods
    {
        public readonly static Vector2 one = new(1, 1);
        public readonly static Vector2 zero = new(0, 0);
        
        private const float Epsilon = float.Epsilon;

        public static Vector2 up = new(0, -1);
        public static Vector2 down = new(0, 1);
        public static Vector2 left = new(-1, 0);
        public static Vector2 right = new(1, 0);

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
        public static float DistanceFrom(this Vector2 v,  Vector2 a)
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
        public static void Rotate(this Vector2 v, float angle)
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
        #region Numbers
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
        public static void Clamp(this ref Vector2 v, Vector2 min, Vector2 max)
        {
            v.X = v.X.Clamp(min.X, max.X);
            v.Y = v.Y.Clamp(min.Y, max.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Wrap(this Vector2 v, Vector2 max) { v.X = v.X.Wrapped(max.X); v.Y = v.Y.Wrapped(max.Y); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Wrapped(this Vector2 v, Vector2 max) => new(v.X.Wrapped(max.X), v.Y.Wrapped(max.Y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithin(this Vector2 v, Vector2 min, Vector2 max) => v.X.IsWithin(min.X, max.X) && v.Y.IsWithin(min.Y, max.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinMaxExclusive(this Vector2 v, Vector2 min, Vector2 max) => v.X.IsWithinMaxExclusive(min.X ,max.X) && v.Y.IsWithinMaxExclusive(min.Y, max.Y);
     

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float v, float min, float max) => MathF.Min(max, MathF.Max(v, min));
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
        public static bool IsWithin(this float v, float min, float max) => v >= min && v <= max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinMaxExclusive(this float v, float min, float max) => v >= min && v < max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDivideSafe(this float v) => v == 0 ? Epsilon : v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakeDivideSafe(this float[] v) { for(int i = 0; i < v.Length; i++) v[i] = v[i].GetDivideSafe(); }
        #endregion
        #region Vectors
        public static Vector2 ToVector(this string input)
        {
            var x = input.Split(',').First().ToInt();
            var y = input.Split(',').Last().ToInt();
            return new Vector2(x, y);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transformed(this Vector2 v, Matrix3x2 matrix) => Vector2.Transform(v, matrix);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this System.Windows.Point v) => new((float)v.X, (float)v.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transform(this ref Vector2 v, Matrix3x2 matrix) =>  v = Vector2.Transform(v, matrix);
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
            v =  v / v.Length();
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
        #endregion
        #region Matrices
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x2 Inverted(this Matrix3x2 matrix)
        {
            Matrix3x2.Invert(matrix, out matrix);
            return matrix;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invert(this ref Matrix3x2 matrix)
        {
            Matrix3x2.Invert(matrix, out matrix);
        }
        #endregion
        #region Strings
        /// <summary>
        /// Takes a string of any format and returns a integer value. If the string contains no number chars, it will return -1.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>an integer of value based on the order and frequency of numbers in the input string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this string input)
        {
            string intResult = "";

            foreach (var x in input)
                if (Constants.int_chars.Contains(x)) 
                    intResult += x;

            return intResult.Length == 0 ? -1 : int.Parse(intResult);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFloat(this string input) => float.Parse(input);
        /// <summary>
        /// Since the assets system handles the file 
        /// 
        /// , this format is only relevant to the naming convention used for files.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A formatted version of the string that will not cause file-saving errors</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToFileNameFormat(this string input)
        {
            var output = "";
            foreach (var _char in input)
            {
                if (!Constants.unsupported_chars.Contains(_char)) output += _char;
            }
            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normal_LHS(this Vector2 v)
        {
            float x = v.X;
            v.X = v.Y;
            v.Y = -x;
            v.Normalize();
            return v; 
        }
        #endregion
        #region Arrays
        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <returns> A one dimensional array containing all the elements of the two-dimensional array passed in.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Flatten<T>(this T[,] array)
        {
            List<T> result = new(array.GetLength(0) + array.GetLength(1));
            foreach (var x in array)
                result.Add(x);
            return result.ToArray();
        }
        #endregion
        #region Image
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Windows.Point GetNormalizedPoint(this System.Windows.Controls.Image img, System.Windows.Point pos)
        {
            pos.X /= img.ActualWidth;
            pos.Y /= img.ActualHeight;
            return pos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ToBitmap(this Pixel[,] colors)
        {
            int sizeX = colors.GetLength(0);
            int sizeY = colors.GetLength(1);

            var bitmap = new Bitmap(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    bitmap.SetPixel(x, y, colors[x, y]);

            return bitmap;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Windows.Media.PixelFormat ToMediaFormat(this System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return System.Windows.Media.PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr32;
            }
            throw new NotImplementedException($"No Media.PixelFormat implemented Imaging.PixelFormat: {sourceFormat.ToString()}");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rectangle Rect(this Bitmap bmp) => new Rectangle(0, 0, bmp.Width, bmp.Height);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pixel Lerp(this Pixel A, Pixel B, float T)
        {
            T = Math.Max(0, Math.Min(1, T));
            int r = (int)Math.Round(A.r + (B.r - A.r) * T);
            int g = (int)Math.Round(A.g + (B.g - A.g) * T);
            int b = (int)Math.Round(A.b + (B.b - A.b) * T);
            int a = (int)Math.Round(A.a + (B.a - A.a) * T);
            return Color.FromArgb(a, r, g, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<FieldInfo> GetSerializedFields(this Component component) =>
            from FieldInfo field in component.GetType().GetRuntimeFields()
            from CustomAttributeData data in field.CustomAttributes
            where data.AttributeType == typeof(FieldAttribute)
            select field;
        #endregion
    }
}
