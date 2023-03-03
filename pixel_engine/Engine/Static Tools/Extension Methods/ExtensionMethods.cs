
namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class ExtensionMethods
    {
        public static float DistanceSquared(Vector2 a, Vector2 b)
        {
            return (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);
        }
        public static float SqrDistanceFrom(this Vector2 a, Vector2 v)
        {
            return DistanceSquared(a, v);
        }
        public static float DistanceFrom(this Vector2 v,  Vector2 a)
        {
            return v.Distance(a);
        }
        public static float Distance(this Vector2 a, Vector2 b)
        {
            var distanceSquared = DistanceSquared(a, b);
            return CMath.Sqrt(distanceSquared);
        }
        public static float Dot(this Vector2 a, Vector2 b)
        {
            return (a.X * b.X) + (a.Y * b.Y);
        }
        public static float Length(this Vector2 v) => MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        public static Vector2 Rotated(this Vector2 v, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector2(cos * v.X - sin * v.Y, sin * v.X + cos * v.Y);
        }
        public static void Rotate(this Vector2 v, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float newX = cos * v.X - sin * v.Y;
            float newY = sin * v.X + cos * v.Y;
            v.X = newX;
            v.Y = newY;
        }
        public static float SqrMagnitude(this Vector2 v)
        {
            var product = MathF.FusedMultiplyAdd(v.X, v.X, v.Y * v.Y);
            return product; 
        }
        public readonly static Vector2 one = new(1, 1);
        public readonly static Vector2 zero = new(0, 0);
        public static Vector2 up = new(0, -1);
        public static Vector2 down = new(0, 1);
        public static Vector2 left = new(-1, 0);
        public static Vector2 right = new(1, 0);


        private const float Epsilon = float.Epsilon;
        #region Numbers
        public static bool WithinRange(this float v, float min, float max) { return v <= max && v >= min; }
        public static bool WithinRange(this int v, int min, int max) { return v <= max && v >= min; }
        public static double Clamp(this double v, double min, double max) => Math.Min(max, Math.Max(v, min));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Squared(this double v) => v * v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Squared(this float v) => v * v;
        public static void Clamp(this ref Vector2 v, Vector2 min, Vector2 max)
        {
            v.X = v.X.Clamp(min.X, max.X);
            v.Y = v.Y.Clamp(min.Y, max.Y);
        }
        public static void Wrap(this Vector2 v, Vector2 max) { v.X = v.X.Wrapped(max.X); v.Y = v.Y.Wrapped(max.Y); }
        public static Vector2 Wrapped(this Vector2 v, Vector2 max) => new(v.X.Wrapped(max.X), v.Y.Wrapped(max.Y));
        public static bool IsWithin(this Vector2 v, Vector2 min, Vector2 max) => v.X.IsWithin(min.X, max.X) && v.Y.IsWithin(min.Y, max.Y);
        public static bool IsWithinMaxExclusive(this Vector2 v, Vector2 min, Vector2 max) => v.X.IsWithinMaxExclusive(min.X ,max.X) && v.Y.IsWithinMaxExclusive(min.Y, max.Y);
        public static float Clamp(this float v, float min, float max) => MathF.Min(max, MathF.Max(v, min));
        public static float Wrapped(this float v, float max)
        {
            float result = v - max * (float)Math.Floor(v / max);
            return result < 0 ? result + max : result;
        }
        public static bool IsWithin(this float v, float min, float max) => v >= min && v <= max;
        public static bool IsWithinMaxExclusive(this float v, float min, float max) => v >= min && v < max;
        public static float GetDivideSafe(this float v) => v == 0 ? Epsilon : v;
        public static void MakeDivideSafe(this float[] v) { for(int i = 0; i < v.Length; i++) v[i] = v[i].GetDivideSafe(); }
        #endregion
        #region Vectors
        public static void GetDivideSafeRef(this ref Vector2 v)
        { 
            v.X = v.X.GetDivideSafe();
            v.Y = v.Y.GetDivideSafe();
        }
        public static void GetDivideSafe(this Vector2 v)
        {
            v.X = v.X.GetDivideSafe();
            v.Y = v.Y.GetDivideSafe();
        }
        public static Vector2 WithValue(this Vector2 v, int? x = null, int? y = null) { return new Vector2(x ?? v.X, y ?? v.Y); }
        public static Vector2 WithValue(this Vector2 v, float? x = null, float? y = null) { return new Vector2(x ?? v.X, y ?? v.Y); }
        public static Vector2 WithScale(this Vector2 v, float x = 1, float y = 1) { return new Vector2(v.X * x, v.Y * y); }
        public static double Sum(this Vector2 v) => v.X + v.Y;
        public static double Sum(this Vector3 v) => v.X + v.Y + v.Z;
        
        /// <summary>
        ///  TODO: fix possible  'divide by zero'
        ///   Normalize a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A normalized Vector from the length of the current</returns>
        public static Vector2 Normalized(this Vector2 v)
        {
            if (v.Equals(Vector2.Zero))
                return Vector2.Zero;
            return v / v.Length();
        }
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
        #region Strings
        /// <summary>
        /// Takes a string of any format and returns a integer value. If the string contains no number chars, it will return -1.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>an integer of value based on the order and frequency of numbers in the input string.</returns>
        public static int ToInt(this string input)
        {
            string intResult = "";

            foreach (var x in input)
                if (Constants.int_chars.Contains(x)) 
                    intResult += x;

            return intResult.Length == 0 ? -1 : int.Parse(intResult);
        }
        public static float ToFloat(this string input) => float.Parse(input);
        /// <summary>
        /// Since the assets system handles the file 
        /// 
        /// , this format is only relevant to the naming convention used for files.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A formatted version of the string that will not cause file-saving errors</returns>
        public static string ToFileNameFormat(this string input)
        {
            var output = "";
            foreach (var _char in input)
            {
                if (!Constants.unsupported_chars.Contains(_char)) output += _char;
            }
            return output;
        }

        public static Vector2 Normal_LHS(this Vector2 v)
        {
            return new Vector2(v.Y, -v.X).Normalized();
        }
        #endregion
        #region Arrays
        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <returns> A one dimensional array containing all the elements of the two-dimensional array passed in.</returns>
        public static T[] Flatten<T>(this T[,] array)
        {
            List<T> result = new(array.GetLength(0) + array.GetLength(1));
            foreach (var x in array)
                result.Add(x);
            return result.ToArray();
        }
        #endregion
        #region Image
        public static System.Windows.Point GetNormalizedPoint(this System.Windows.Controls.Image img, System.Windows.Point pos)
        {
            pos.X /= img.ActualWidth;
            pos.Y /= img.ActualHeight;
            return pos;
        }
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
        #endregion

        public static Pixel Lerp(this Pixel A, Pixel B, float T)
        {
            T = Math.Max(0, Math.Min(1, T));
            int r = (int)Math.Round(A.r + (B.r - A.r) * T);
            int g = (int)Math.Round(A.g + (B.g - A.g) * T);
            int b = (int)Math.Round(A.b + (B.b - A.b) * T);
            int a = (int)Math.Round(A.a + (B.a - A.a) * T);
            return Color.FromArgb(a, r, g, b);
        }

        public static IEnumerable<FieldInfo> GetSerializedFields(this Component component) =>
            from FieldInfo field in component.GetType().GetRuntimeFields()
            from CustomAttributeData data in field.CustomAttributes
            where data.AttributeType == typeof(FieldAttribute)
            select field;

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
        public static Rectangle Rect(this Bitmap bmp) => new Rectangle(0, 0, bmp.Width, bmp.Height);
        internal static T Clone<T>(this T component) where T : Component => component.GetShallowClone<T>();
    }
}
