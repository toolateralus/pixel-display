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
        private protected static List<char> disallowed_chars = new()
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
        private protected static string[] typeIdentifiers = new string[] 
        {
            "vec:",
            "int:",
            "str:",
            "float:",
            "bool:",
        };
        
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
        private static string String(string arg0)
        {
            foreach (var x in arg0)
                if (disallowed_chars.Contains(x))
                    arg0 = arg0.Replace(x, (char)0);
            return arg0;
        }

        internal static void TryCallLine(string line, Command[] commands)
        {
            _ = ParseArguments(line, out string[] args);
            _ = ParseLoopParams(line, out string loop_param);

            int count = loop_param.ToInt();

            foreach (var command in commands)
                if (command.Equals(line))
                    ExecuteCommand(args, count, command);
        }

        private static void ExecuteCommand(string[] args, int count, Command command)
        {
            if (args.Length > 0)
            {
                List<object> init_args = new();
                
                for (int i = 0; i < args.Length; ++i)
                {
                    object? parse_arg = ParseParam(args[i]);

                    if (parse_arg != null)
                        init_args.Add(parse_arg);
                }
                command.args = init_args.ToArray();
                command.Invoke();
            }
            for (int i = 0; i < count; ++i)
            {
                command.Invoke();
            }
        }

        public static object? ParseParam(string arg)
        {
            // this string gets treated like a null/void variable.
            object? outArg = "";

            arg = RemoveUnwantedChars(arg);
            outArg = ConvertToObjectAndRemoveTypeArg(arg, outArg);
            return outArg;
        }
        private static object ConvertToObjectAndRemoveTypeArg(string arg, object outArg)
        {
            foreach (var identifier in typeIdentifiers)
                if (arg.Contains(identifier))
                {
                    switch (identifier)
                    {
                        case "vec:":
                            arg = arg.Replace("vec:", "");
                            outArg = Vec2(arg);
                            break;
                        case "int:":
                            arg = arg.Replace("int:", "");
                            outArg = int.Parse(arg);
                            break;
                        case "str:":
                            arg = ((string)arg).Replace("str:", "");
                            outArg = String(arg);
                            break;
                        case "float:":
                            arg = arg.Replace("float:", "");
                            outArg = float.Parse(arg);
                            break;
                        case "bool:":
                            arg = arg.Replace("bool:", "");
                            outArg = bool.Parse(arg);
                            break;
                        default:
                            arg = "";
                            outArg = new object(); 
                            break;
                    }
                }

            return outArg;
        }
        internal static string ParseLoopParams(string input, out string repeaterArgs)
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
            cmd_without_args = input.Split('(')[0] + ';';

            if (cmd_has_args)
                arguments = parseArgumentsReturnCommand(input, ref args_str);


            return cmd_without_args;

            static string remove_parentheses(string input)
            {
                if (input.Contains('('))
                     input =  input.Replace("(", "");
                if (input.Contains(')'))
                     input = input.Replace(")", "");
                return input; 
            }
            static string[] split_args_at_commas_with_trailing_whitespace(string args_str)
            {
                return args_str.Split(", ");
            }
            static string get_args_string(string input, string args_str, int argStartIndex, int argEndIndex)
            {
                for (int i = argStartIndex; i < argEndIndex; ++i)
                    args_str += input[i];
                return args_str;
            }
            static void args_indices(string input, out int argStartIndex, out int argEndIndex)
            {
                argStartIndex = input.IndexOf('(');
                argEndIndex = input.IndexOf(';');
            }
            static string[] parseArgumentsReturnCommand(string input, ref string args_str)
            {
                string[] arguments;
                int argStartIndex, argEndIndex;
                args_indices(input, out argStartIndex, out argEndIndex);
                args_str = get_args_string(input, args_str, argStartIndex, argEndIndex);

                // causes whitespace to be neccessary to indicate void or null argument,
                // might cause issues being unimplemented
                // args_str = remove_parentheses(args_str);

                arguments = split_args_at_commas_with_trailing_whitespace(args_str);
                return arguments;
            }
        }
    }
}