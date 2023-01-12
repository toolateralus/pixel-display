using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace pixel_editor
{
    public class CommandParser 
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

        private static string RemoveUnwantedChars(string? arg0)
        {
            foreach (var _char in arg0)
                if (disallowed_chars.Contains(_char))
                    arg0 = arg0.Replace($"{_char}", "");
            return arg0;
        }

        internal static void TryParseLine(string line, Command command)
        {
            string withoutArgs = ParseArguments(line, out string pArgs);
            withoutArgs = ParseIterator(line, out string rArgs);
            int count = rArgs.ToInt();

            // single execution paramaterless command; 
            if (count == 0 && pArgs.Length == 0)
            {
                command.Invoke();
                return;
            }
            // command with params; 
            if (pArgs.Length > 0)
            {
               
                string args = (string)Parse<string>(pArgs);

                command.args = new object[] 
                {
                    args
                };
                command.Invoke(); 
            }
            for (int i = 0; i < count; ++i)  command.Invoke();
        }
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

        internal static string ParseIterator(string input, out string repeaterArgs)
        {
            string withoutArgs = "";
            repeaterArgs = ""; 
            if (input.Contains('$'))
            {
                int indexOfStart = input.IndexOf('$');
                int indexOfEnd = input.IndexOf(';');
                for (int i = indexOfStart; i < indexOfEnd; ++i)
                    repeaterArgs += input[i];
                if(repeaterArgs.Length > 0)
                    withoutArgs = input.Replace(repeaterArgs, "");
            }
            else withoutArgs = input;
            return withoutArgs;
        }
        internal static string ParseArguments(string input, out string arguments)
        {
            arguments = "";
            string cmd_without_args = "";

            bool cmd_has_args = input.Contains('(') && input.Contains(')'); 

            if (cmd_has_args)
            {
                int argStartIndex = input.IndexOf('(');
                int argEndIndex = input.IndexOf(';');


                for (int i = argStartIndex; i < argEndIndex; ++i)
                    arguments += input[i];

                var splitParams = arguments.Split(',');


                if(arguments.Length > 0)
                    cmd_without_args = input.Replace(arguments, "");
            }
            return cmd_without_args;
        }
    }
}