
namespace pixel_renderer
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public static class ExtensionMethods
    {
        public static bool WithinRange(this float v, float min, float max) { return v <= max && v >= min; }
        public static Vec2 WithValue(this Vec2 v, int? x = null, int? y = null) { return new Vec2(x ?? v.x, y ?? v.x); }
        public static Vec2 WithScale(this Vec2 v, int x = 1, int y = 1) { return new Vec2(v.x * x, v.y * y); }
        /// <summary>
        /// Takes a string of any format and returns a numerical value. If the string contains no number chars, it will return -1.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>an integer of value based on the order and frequency of numbers in the input string.</returns>
        public static int ToInt(this string input)
        {
            
                string intResult = "";

            foreach (var x in input)
                if (Settings.int_chars_array.Contains(x)) intResult += x;

            if (intResult.Length == 0) return -1;

            return int.Parse(intResult);
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
                if (!Settings.unsupported_char_array.Contains(_char)) output += _char; 
            }
            return output; 
        } 
        public static double Sum(this Vec2 v) => v.x + v.y;
        public static double Sum(this Vec3 v) => v.x + v.y + v.z;
        public static float Distance(this Vec2 v, Vec2 end) => (v - end).Length;
        /// <summary>
        ///  TODO: fix possible  'divide by zero'
        ///   Normalize a vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A normalized Vector from the length of the current</returns>
        public static Vec2 Normalize(this Vec2 v) => v / v.Length;
        public static double Clamp(this double v, double min, double max)
        {
            return Math.Min(max, Math.Max(v, min));
        }

    }
}
