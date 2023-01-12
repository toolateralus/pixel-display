using pixel_renderer;
using System;
using System.CodeDom;
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

        internal static void TryCallLine(string line, Command command)
        {
            string withoutArgs = ParseArguments(line, out string[] args);
            withoutArgs = ParseIterativeArgs(line, out string rArgs);
            int count = rArgs.ToInt();

            // single execution paramaterless command; 
            if (count == 0 && args.Length == 0)
            {
                command.Invoke();
                return;
            }
            // command with params; 
            if (args.Length > 0)
            {
                List<object> init_args = new List<object>{ };
                for(int i =0; i < args.Length; ++i)
                {
                    object? parse_arg = Parse<string>(args[i]);
                    init_args.Add(parse_arg);
                }
                command.args = init_args.ToArray(); 
                command.Invoke(); 
            }
            for (int i = 0; i < count; ++i)  command.Invoke();
        }
        public static object? Parse<T>(string arg = null) where T : class 
        {
                arg = RemoveUnwantedChars(arg);
                if (typeof(T) == typeof(string))
                    return String(arg);
                if (typeof(T) == typeof(int))
                    return Int(arg);
                if (typeof(T) == typeof(Vec2))
                    return Vec2(arg);
                throw new ArgumentException("Console command used an Argument that is of a type that is not currently supported. Sorry XD");
        }

        internal static string ParseIterativeArgs(string input, out string repeaterArgs)
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
        internal static string ParseArguments(string input, out string[] arguments)
        {
            arguments = new string[]{};
            var args_str = ""; 
            string cmd_without_args = "";

            bool cmd_has_args = input.Contains('(') && input.Contains(')'); 

            if (cmd_has_args)
                arguments = parseArgumentsReturnCommand(input, ref args_str, ref cmd_without_args);
            return cmd_without_args;

            static string remove_parentheses(string input)
            {
                if (input.Contains('('))
                     input =  input.Replace("(", "");
                if (input.Contains(')'))
                     input = input.Replace(")", "");
                return input; 
            }
            static string[] split_args_at_commas(string args_str)
            {
                return args_str.Split(',');
            }
            static string get_args_string(string input, string args_str, int argStartIndex, int argEndIndex)
            {
                for (int i = argStartIndex; i < argEndIndex; ++i)
                    args_str += input[i];
                return args_str;
            }
            static void argsIndices(string input, out int argStartIndex, out int argEndIndex)
            {
                argStartIndex = input.IndexOf('(');
                argEndIndex = input.IndexOf(';');
            }
            static string clean_up(string input, string[] arguments, string cmd_without_args)
            {
                if (arguments.Length > 0)
                    foreach (var arg in arguments)
                    {
                        if (arg.Length == 0) continue;
                        cmd_without_args = input.Replace(arg, "");
                        cmd_without_args = cmd_without_args.Replace(",", "");
                    }

                return cmd_without_args;
            }

            static string[] parseArgumentsReturnCommand(string input, ref string args_str, ref string cmd_without_args)
            {
                string[] arguments;
                int argStartIndex, argEndIndex;

                argsIndices(input, out argStartIndex, out argEndIndex);
                args_str = get_args_string(input, args_str, argStartIndex, argEndIndex);
                //args_str = remove_parentheses(args_str);
                arguments = split_args_at_commas(args_str);
                cmd_without_args = clean_up(input, arguments, cmd_without_args);
                cmd_without_args = remove_parentheses(cmd_without_args);
                return arguments;
            }
        }
    }
}