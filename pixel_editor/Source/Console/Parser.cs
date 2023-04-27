using Pixel;
using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows.Documents;

namespace pixel_editor
{
    public class CommandParser
    {
        private const char Loop = '$';
        private const char EndLine = ';';
        private const char ArgumentsStart = '(';
        private const char ArgumentsEnd = ')';
        private const string ParameterSeperator = ", ";

        // use list for linq methods
      
       
        private static void ExecuteCommand(string[] args, Command command)
        {
            try
            {
                if (args.Length > 0)
                {
                    List<object> parsed_objects = new();

                    for (int i = 0; i < args.Length; ++i)
                    {
                        object? parsed_arg_obj = ParseParam(args[i], command, i);

                        if (parsed_arg_obj != null)
                            parsed_objects.Add(parsed_arg_obj);
                    }
                    command.args = parsed_objects.ToArray();
                    command.Invoke();
                }
                else command.Invoke();
            }
            catch (Exception e)
            {
                command.error = e.Message;
            }
        }
        public static void TryCallLine(string line)
        {
            ParseArguments(line, out string[] args, out _);
            line = ParseLoopParams(line, out string loop_param);
            var commands = Console.Current.LoadedCommands;
            foreach (Command command in commands)
                if (command.Equals(line))
                {
                    ExecuteCommand(args, command);
                    if (command.error != null)
                    {
                        Runtime.Log(command.error);
                        command.error = null;
                        continue;
                    }
                    Command.Success(command.syntax);
                }
        }

        public static void TryParse(string input, out List<object> value)
        {
            value = new();
            for (int i = 0; i < 5; ++i)
                switch (i)
                {
                    // string
                    case 0:
                        try
                        {
                            value.Add(input);
                        }
                        catch (Exception) { };
                        continue;
                    // bool
                    case 1:
                        try
                        {
                            if (bool.TryParse(input, out var val))
                                value.Add(val);
                        }
                        catch (Exception)
                        {

                        };
                        continue;
                    // int
                    case 2:
                        try
                        {
                            if (int.TryParse(input, out var val))
                                value.Add(val);
                        }
                        catch (Exception) { };
                        continue;
                    // float
                    case 3:
                        try
                        {
                            if (float.TryParse(input, out var val))
                                value.Add(val);
                        }
                        catch (Exception) { };
                        continue;
                    // vec2
                    case 4:
                        try
                        {
                            Vector2 vec = input.ToVector();
                            value.Add(vec);

                        }
                        catch (Exception) { };
                        continue;

                }
        }
        public static object? ParseParam(string arg, Command command, int index)
        {
            // important : this string gets treated like a null/void variable. //
            object? outArg = "";

            arg = RemoveUnwantedChars(arg);

            if (command.argumentTypes is null || command.argumentTypes.Length < index)
                return outArg;

            try
            {
                switch (command.argumentTypes[index])
                {
                    case "vec:":
                        outArg = Vec2(arg);
                        break;
                    case "int:":
                        outArg = int.Parse(arg);
                        break;
                    case "str:":
                        outArg = String(arg);
                        break;
                    case "float:":
                        outArg = float.Parse(arg);
                        break;
                    case "bool:":
                        outArg = bool.Parse(arg);
                        break;
                }
            }
            catch (Exception e)
            {
                Runtime.Log(e.Message);
            }
            return outArg;
        }
        public static string ParseLoopParams(string input, out string repeaterArgs)
        {
            string withoutArgs = "";
            repeaterArgs = "";
            if (input.Contains(Loop))
            {
                int indexOfStart = input.IndexOf(Loop);
                int indexOfEnd = input.IndexOf(EndLine);

                for (int i = indexOfStart; i < indexOfEnd; ++i)
                    repeaterArgs += input[i];

                if (repeaterArgs.Length > 0)
                    withoutArgs = input.Replace(repeaterArgs, "");
            }
            else withoutArgs = input;
            return withoutArgs;
        }
        public static void ParseArguments(string input, out string[] arguments, out string commandPhrase)
        {
            var args_str = "";
            arguments = Array.Empty<string>();

            commandPhrase = GetCmdPhrase(input);

            if (HasArgs(input))
                arguments = GetParameterArray(input, ref args_str);


        }
        private static Vector2 Vec2(string? arg0)
        {
            string[] values = arg0.Split(',');

            if (values.Length < 2)
                return Vector2.Zero;

            var x = RemoveUnwantedChars(values[0]).ToFloat();
            var y = RemoveUnwantedChars(values[1]).ToFloat();

            return new Vector2(x, y);
        }
        private static string String(string arg0)
        {
            foreach (var x in arg0)
                if (Constants.disallowed_chars.Contains(x))
                    arg0 = arg0.Replace(x, (char)0);
            return arg0;
        }
        private static string RemoveUnwantedChars(string? arg0)
        {
            foreach (var _char in arg0)
                if (Constants.disallowed_chars.Contains(_char))
                    arg0 = arg0.Replace($"{_char}", "");
            return arg0;
        }
        public static string[] SplitArgsIntoParams(string args_str) => args_str.Split(ParameterSeperator);
        public static string GetCmdPhrase(string input)
        {
            return input.Split(ArgumentsStart)[0] + EndLine;
        }
        public static string GetArguments(string input, string arguments, int argStartIndex, int argEndIndex)
        {
            for (int i = argStartIndex; i < argEndIndex; ++i)
                arguments += input[i];
            return arguments;
        }
        public static void GetArgumentIndices(string input, out int argStartIndex, out int argEndIndex)
        {
            argStartIndex = input.IndexOf(ArgumentsStart);
            argEndIndex = input.IndexOf(EndLine);
        }
        public static string[] GetParameterArray(string input, ref string args_str)
        {
            string[] arguments;
            GetArgumentIndices(input, out int argStartIndex, out int argEndIndex);
            args_str = GetArguments(input, args_str, argStartIndex, argEndIndex);
            arguments = SplitArgsIntoParams(args_str);
            return arguments;
        }
        public static bool HasArgs(string input)
        {
            return input.Contains(ArgumentsStart) && input.Contains(ArgumentsEnd);
        }
    }
}