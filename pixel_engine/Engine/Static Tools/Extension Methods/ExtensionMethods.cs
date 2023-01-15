
namespace pixel_renderer
{
    using pixel_renderer.Assets;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;

    public static class ExtensionMethods
    {
        #region Numbers
        public static bool WithinRange(this float v, float min, float max) { return v <= max && v >= min; }
        public static double Clamp(this double v, double min, double max) => Math.Min(max, Math.Max(v, min));
        public static float Clamp(this float v, float min, float max) => MathF.Min(max, MathF.Max(v, min));
        public static float Wrapped(this float v, float max) => v % max >= 0 ? v % max : v % max + max;
        public static bool IsWithin(this float v, float min, float max) => v >= min && v <= max;
        public static bool IsWithinMaxExclusive(this float v, float min, float max) => v >= min && v < max;
        public static float GetDivideSafe(this float v) => v == 0 ? float.Epsilon : v;
        public static void MakeDivideSafe(this float[] v) { for(int i = 0; i < v.Length; i++) v[i] = v[i].GetDivideSafe(); }
        #endregion
        #region Vectors
        public static Vec2 GetDivideSafe(this Vec2 v) => new(v.x.GetDivideSafe(), v.y.GetDivideSafe());
        public static Vec2 WithValue(this Vec2 v, int? x = null, int? y = null) { return new Vec2(x ?? v.x, y ?? v.x); }
        public static Vec2 WithValue(this Vec2 v, float? x = null, float? y = null) { return new Vec2(x ?? v.x, y ?? v.x); }
        public static Vec2 WithScale(this Vec2 v, int x = 1, int y = 1) { return new Vec2(v.x * x, v.y * y); }
        public static double Sum(this Vec2 v) => v.x + v.y;
        public static double Sum(this Vec3 v) => v.x + v.y + v.z;
        public static float Distance(this Vec2 v, Vec2 end) => (v - end).Length();
        public static string AsString(this Vec2 v)
        {
            return $" ({v.x},{v.y})";
        }
        /// <summary>
        ///  TODO: fix possible  'divide by zero'
        ///   Normalize a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A normalized Vector from the length of the current</returns>
        public static Vec2 Normalize(this Vec2 v)
        {
            if (v.Equals(Vec2.zero))
                return Vec2.zero;

            return v / v.Length();
        }
        #endregion
        #region Strings
        /// <summary>
        /// Takes a string of any format and returns a numerical value. If the string contains no number chars, it will return -1.
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
        /// <summary>
        /// Since the assets system handles the file extension, this format is only relevant to the naming convention used for files.
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
        #region Node

        public static List<NodeAsset> ToNodeAssets(this List<Node> input)
        {
            List<NodeAsset> output = new();
            foreach (var node in input)
                output.Add(node.ToAsset());
            return output;
        }
        public static List<Node> ToNodeList(this List<NodeAsset> input)
        {
            List<Node> output = new();
            foreach (var asset in input)
                output.Add(asset.Copy());
            return output;
        }

        #endregion



        public static IEnumerable<FieldInfo> GetSerializedFields(this Component component) =>
            from FieldInfo field in component.GetType().GetRuntimeFields()
            from CustomAttributeData data in field.CustomAttributes
            where data.AttributeType == typeof(FieldAttribute)
            select field;
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
            return new System.Windows.Media.PixelFormat();
        }
        public static Rectangle Rect(this Bitmap bmp) => new Rectangle(0, 0, bmp.Width, bmp.Height);
    }
}
