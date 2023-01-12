using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace pixel_editor
{
    public class CommandArgsParser 
    {
        // for the final cleanup of commands before execution
        static List<char> disallowed_chars = new()
        {
            ';',
            '\'',
            '\0',
            '\\',
            '/',
            '(',
            ')',
            '"',
        };
        public static object? Parse<T>(string? arg0 = null) where T: class
        {
            arg0 = RemoveUnwantedChars(arg0);

            if (typeof(T) == typeof(string))
                return String(arg0);

            if (typeof(T) == typeof(int))
                return Int(arg0);

            if (typeof(T) == typeof(Vec2))
                return Vec2(arg0);

            return null;
        }
        private static string RemoveUnwantedChars(string? arg0)
        {
            foreach (var _char in arg0)
                if (disallowed_chars.Contains(_char))
                    arg0 = arg0.Replace($"{_char}", "");
            return arg0;
        }
        private static Vec2 Vec2(string? arg0)
        {
            string[] values = arg0.Split(',');
            
            if(values.Length < 2 )
             return pixel_renderer.Vec2.zero; 

            var x = values[0].ToInt();
            var y = values[1].ToInt();

            return new Vec2(x,y);
        }
        private static int Int(string? arg0) => arg0.ToInt(); 
        private static string String(string arg0)
        {
            foreach (var x in arg0)
                if (disallowed_chars.Contains(x))
                    arg0.Replace(x, (char)0);
            return arg0;
        }
    }
}