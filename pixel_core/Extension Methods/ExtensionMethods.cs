using Pixel.Statics;
using Pixel.Types.Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Pixel
{
    public static class ExtensionMethods
    {
        #region Matrices
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x2 Inverted(this Matrix3x2 matrix)
        {
            Matrix3x2.Invert(matrix, out matrix);
            return matrix;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invert(this ref Matrix3x2 matrix) =>
            Matrix3x2.Invert(matrix, out matrix);
        public static void SetScale(this ref Matrix3x2 matrix, Vector2 scale)
        {
            matrix.M11 = scale.X;
            matrix.M22 = scale.Y;
        }
        #endregion

        #region Strings
        public static Vector2 ToVector(this string input)
        {
            var split = input.Split(',');

            if (!split.Any())
                return default;

            string x = split.First();
            string y = split.Last();

            foreach (var _char in Constants.disallowed_chars)
            {
                if (x.Contains(_char))
                    x = x.Replace($"{_char}", "");

                if (y.Contains(_char))
                    y = y.Replace($"{_char}", "");
            }

            float xF = float.Parse(x);
            float yF = float.Parse(y);

            return new Vector2(xF, yF);

        }
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
                if (char.IsDigit(x))
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
        public static Bitmap ToBitmap(this Color[,] colors)
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
        public static Color Lerp(this Color A, Color B, float T)
        {
            T = Math.Max(0, Math.Min(1, T));
            int r = (int)Math.Round(A.r + (B.r - A.r) * T);
            int g = (int)Math.Round(A.g + (B.g - A.g) * T);
            int b = (int)Math.Round(A.b + (B.b - A.b) * T);
            int a = (int)Math.Round(A.a + (B.a - A.a) * T);
            return System.Drawing.Color.FromArgb(a, r, g, b);
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
