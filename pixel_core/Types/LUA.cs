﻿using KeraLua;
using Pixel.Statics;
using Pixel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Pixel
{
    /// <summary>
    /// A mostly static class functioning as a wrapper for KeraLua Lua usage. Also contains all Pixel LuaFunctions aka PixelLua library :D
    /// </summary>
    public class LUA
    {
        public static List<LuaFunction> functions_list = new();
        static Dictionary<string, object> env_vars = new();
        private static readonly KeraLua.Lua state = new();
        public static bool envMode = false;

        private static void PrintLUA(string print)
        {
            FromString($"print(\"{print}\")");
        }
        public LUA() => RefreshFunctions();
        public void RefreshFunctions()
        {
            var type = GetType();
            var methods = type.GetRuntimeMethods();
            foreach (var method in methods)
                if (method.ReturnType == typeof(LuaFunction))
                {
                    LuaFunction function = (LuaFunction)method.Invoke(null, null);
                    functions_list.Add(function);
                    state.Register(method.Name, function);
                }
        }
        public static (bool result, string err) FromString(string luaString)
        {
            if (luaString is null || luaString is "")
            {
                Interop.Log("Lua component was called to run but it had no valid lua code to run.");
                return (true, "No code found");
            }
            var result = state.DoString(luaString);
            if (result)
                return (false, "nil");
            return (true, state.ToString(1));
        }
        public static bool FromFile(string fileName)
        {
            var result = state.LoadFile(fileName);
            if (result != LuaStatus.OK)
            {
                Interop.Log(state.ToString(1));
                return false;
            }
            result = state.PCall(0, -1, 0);
            if (result != LuaStatus.OK)
            {
                Interop.Log(state.ToString(1));
                return false;
            }
            return true;
        }
        public static IntPtr GetHandle()
        {
            return state.Handle;
        }
        public static string Environment(string line)
        {
            if (line == "*c();\r\n")
            {
                Interop.ClearConsole();
                return "";
            }

            if (line == "env();\r\n")
            {
                env();
                return "";
            }

            if (line.Contains(' '))
            {
                string[] split = line.Split(' ');

                if (split[0] is not string s1 || s1 is "" || split[1] is not string s2 || s2 is "")
                    return "";


                if (!env_vars.ContainsKey(s1))
                    env_vars.Add(s1, s2);
                else env_vars[s1] = s2;

                Interop.Log($"{s1} : {s2} added to environment vars.");

                return s2;
            }

            return "";
        }
       

        #region pixel_engine lua library (C# functions)
        public static LuaFunction dispose() => new((p) =>
        {
            var ct = env_vars.Count;
            env_vars.Clear();
            FromString($"{ct} environment variables released.");
            return 0;
        });
        public static LuaFunction env() => new((p) =>
        {
            env_vars ??= new();
            envMode = !envMode;
            return 0;
        });
        public static LuaFunction print() => new((p) =>
        {
            var state = KeraLua.Lua.FromIntPtr(p);
            var n = state.GetTop();
            for (int i = 1; i <= n; ++i)
            {
                LuaType type = state.Type(i);
                switch (type)
                {
                    case LuaType.None:
                        break;
                    case LuaType.Nil:
                        Interop.Log("Lua: Nil");
                        break;
                    case LuaType.Boolean:
                        Interop.Log($"Lua: {state.ToBoolean(i)}");
                        break;
                    case LuaType.Number:
                        Interop.Log($"Lua: {state.ToNumber(i)}");
                        break;
                    case LuaType.String:
                        Interop.Log($"Lua: {state.ToString(i)}");
                        break;
                    case LuaType.LightUserData:
                        Interop.Log($"Lua: Light User Data");
                        break;
                    case LuaType.Table:
                        Interop.Log($"Lua: Table");
                        break;
                    case LuaType.Function:
                        Interop.Log($"Lua: Function");
                        break;
                    case LuaType.UserData:
                        Interop.Log($"Lua: User Data");
                        break;
                    case LuaType.Thread:
                        Interop.Log($"Lua: Thread");
                        break;
                    default:
                        break;
                }
            }
            return 0;
        });
        public static LuaFunction getnode() => new((p) =>
        {
            var state = KeraLua.Lua.FromIntPtr(p);
            var n = state.GetTop();
            for (int i = 1; i <= n; ++i)
            {
                LuaType type = state.Type(i);
                switch (type)
                {
                    case LuaType.Number:
                        break;
                    case LuaType.String:
                        string name = state.ToString(i);
                        Stage stage = Interop.Stage;
                        Node result = stage?.FindNode(name);
                        if (result != null)
                        {
                            env_vars.Add(result.Name, result);
                            var print = $"{result.Name} found..loaded at index {env_vars.Count}";
                            PrintLUA(print);
                        }
                        break;
                    default:
                        break;
                }
            }
            return 0;
        });
        public static LuaFunction list_env() => new((p) =>
        {
            var state = KeraLua.Lua.FromIntPtr(p);
            var n = state.GetTop();
            for (int i = 1; i <= n; ++i)
            {
                LuaType type = state.Type(i);
                switch (type)
                {
                    case LuaType.None:
                        foreach (var obj in env_vars)
                        {
                            PrintLUA(obj.Key + '\n');
                        }

                        break;
                    default:
                        break;
                }
            }
            return 0;
        });
        #endregion
    }
    public class Token
    {
        /// <summary>
        /// The type of the token, used for performing operations and executing expressions.
        /// </summary>
        public TokenType Type { get; }
        /// <summary>
        /// the string representation of the token
        /// </summary>
        public string String { get; }
        /// <summary>
        /// A pointer to the object, also for caching.
        /// </summary>
        public Token(TokenType type, string value)
        {
            this.String = value;
            this.Type = type;
        }
    }
    public class Tokenizer
    {
        private readonly string input;
        private int position;
        public Tokenizer(string input)
        {
            this.input = input;
            this.position = 0;
        }
        public Token? GetNextToken()
        {
            if (position >= input.Length)
                return null;

            char currentChar = input[position];

            currentChar = IgnoreWhitespace(currentChar);

            if (char.IsDigit(currentChar))
                return parseDigits();

            if (char.IsLetter(currentChar))
                return parseLetters();

            return parseOperators(currentChar);

            Token? parseLetters()
            {
                StringBuilder identifierBuilder = new StringBuilder();

                while (position < input.Length && char.IsLetterOrDigit(input[position]))
                {
                    identifierBuilder.Append(input[position]);
                    position++;
                }

                string identifier = identifierBuilder.ToString();

                return identifier switch
                {
                    "var" => new Token(TokenType.VarDecl, identifier),
                    "delete" => new Token(TokenType.Delete, identifier),
                    "for" => new Token(TokenType.For, identifier),
                    "if" => new Token(TokenType.If, identifier),
                    "return" => new Token(TokenType.Return, identifier),
                    "null" => new Token(TokenType.Null, identifier),
                    _ => new Token(TokenType.Identifier, identifier),
                };
            }
            Token? parseDigits()
            {
                StringBuilder numberBuilder = new StringBuilder();

                while (position < input.Length && Char.IsDigit(input[position]))
                {
                    numberBuilder.Append(input[position]);
                    position++;
                }

                return new Token(TokenType.Number, numberBuilder.ToString());
            }
            Token? parseOperators(char currentChar)
            {
                switch (currentChar)
                {
                    case '+':
                        position++;
                        return new Token(TokenType.Add, "+");
                    case '-':
                        position++;
                        return new Token(TokenType.Subtract, "-");
                    case '=':
                        position++;
                        return new Token(TokenType.Assignment, "=");
                    case '(':
                        position++;
                        return new Token(TokenType.LeftParen, "(");
                    case ')':
                        position++;
                        return new Token(TokenType.RightParen, ")");
                    case '*':
                        position++;
                        return new Token(TokenType.Multiply, "*");
                    case '/':
                        position++;
                        return new Token(TokenType.Divide, "/");
                }
                return null;
            }

            char IgnoreWhitespace(char currentChar)
            {
                if (currentChar is ' ' && position + 1 < input.Length)
                {
                    position++;
                    currentChar = input[position];
                }

                return currentChar;
            }
        }
    }
    public class ArithmeticInterpreter : IInterpreter
    {
        private Stack<Token> tokens = new();
        private Dictionary<string, (TokenType, object)> variables = new();

        private int index;
        public void PushTokensOntoStack(Stack<Token> tokens)
        {
            this.tokens.Clear();

            while(tokens.Count > 0) 
                this.tokens.Push(tokens.Pop());

            index = 0;
        }
        public double? Evaluate()
        {
            var result = ParseExpression();
            if (index < tokens.Count)
            {
                Interop.Log($"Unexpected token: {tokens.Peek().String}");
                return null;
            }
            return result;
        }

        

        public enum TokenFamily : long
        {
            UNDEFINED = -long.MaxValue,
            KEYWORD = 0,
            OPERATOR = 1,
            VALUE = 2,
            IDENTIFIER = 4,
        }

      
        private TokenFamily CheckFamily(TokenType type)
        {
            switch (type)
            {
                case TokenType.Identifier:
                    return TokenFamily.IDENTIFIER;

                case TokenType.VarDecl:
                case TokenType.Delete:
                case TokenType.Assignment:
                case TokenType.For:
                case TokenType.If:
                case TokenType.Return:
                case TokenType.Null:
                    return TokenFamily.KEYWORD;


                case TokenType.LeftParen:
                case TokenType.RightParen:
                case TokenType.Add:
                case TokenType.Subtract:
                case TokenType.Multiply:
                case TokenType.Divide:
                    return TokenFamily.OPERATOR;

                case TokenType.Number:
                case TokenType.Char:
                    return TokenFamily.VALUE;

                default:
                    return TokenFamily.UNDEFINED;
            };
        }

        private double? ParseExpression()
        {
            var keyword = tokens.Pop();
            var type = keyword.Type;
            var family = CheckFamily(type);
            
            switch (family)
            {
                case TokenFamily.UNDEFINED:
                    Interop.Log(keyword);
                    break;

                case TokenFamily.KEYWORD:
                    return ParseKeyword();

                case TokenFamily.OPERATOR:
                    break;

                case TokenFamily.VALUE:
                    return ParseAddition();

                case TokenFamily.IDENTIFIER:
                    return AccessObject();

            }
            return null;
        }

        private double? AccessObject()
        {
            return null;
        }

        private double? ParseKeyword()
        {

            return TryDeclareVariable();
            double? TryDeclareVariable()
            {   
                var identifier = tokens.Pop();

                if (identifier.Type != TokenType.Identifier)
                    return null;

                var assignment_operator = tokens.Pop();

                if (assignment_operator.Type != TokenType.Assignment)
                    return null;

                var value = tokens.Pop();

                switch (CheckFamily(value.Type))
                {
                    case TokenFamily.UNDEFINED:
                        break;
                    case TokenFamily.KEYWORD:
                        Interop.Log("Invalid variable declaration. Cannot convert {value} to {identifier}");
                        break;

                    case TokenFamily.OPERATOR:
                        Interop.Log("Invalid variable declaration. Cannot convert {value} to {identifier}");
                        break;

                    case TokenFamily.VALUE:
                        switch (value.Type)
                        {
                            case TokenType.Null:
                            case TokenType.Number:
                            case TokenType.Char:
                                Interop.Log($"newly declared variable found as type : {value.Type}");
                                variables.Add(identifier.String, (identifier.Type, value));
                                break;
                        }
                        return null;
                    case TokenFamily.IDENTIFIER:
                        Interop.Log("Invalid variable declaration. Type was not found.");
                        break;
                    default:
                        return null;
                }
                return null;
            }
        }


        private double? ParseAddition()
        {
            var left = ParseMultiplication();

            if (!left.HasValue)
                return null;

            while (HasAdditionOperators())
            {
                var op = tokens.Pop().Type;

                var right = ParseMultiplication();

                if (!right.HasValue)
                    return null;

                left = PerformAddition(left.Value, op, right.Value);
            }
            return left;
        }
        private double? ParseMultiplication()
        {
            var left = ParseUnary();

            if (!left.HasValue)
                return null;

            while (HasMultiplicationOperator())
            {
                var op = tokens.Pop().Type;
                var right = ParseUnary();

                if (!right.HasValue)
                    return null;

                left = PerformMultiplication(left.Value, op, right.Value);
            }
            return left;
        }
        private double? ParseUnary()
        {
            if (tokens.Count == 0)
                return null;

            var token = tokens.Peek();
            double result;

            var init = ParsePrimary();

            if (!init.HasValue)
                return null;

            if (token.Type == TokenType.Subtract)
            {
                tokens.Pop();
                result = -init.Value;
            }
            else
            {
                result = init.Value;
            }

            return result;
        }
        private double? ParsePrimary()
        {
            if (tokens.Count == 0)
                return null;

            var token = tokens.Peek();
            bool isNumber = token.Type == TokenType.Number;
            bool isLeftParentheses = token.Type == TokenType.LeftParen;

            if (isNumber)
            {
                tokens.Pop();
                return double.Parse(token.String);
            }

            if (isLeftParentheses)
            {
                tokens.Pop();
                var result = ParseExpression();
                bool missing_args = tokens.Count == 0 || token.Type != TokenType.RightParen;

                if (missing_args)
                    throw new Exception($"Missing closing parentheses");

                tokens.Pop();
                return result;
            }

            return null;
        }
        private static double PerformMultiplication(double left, TokenType op, double right)
        {
            if (op == TokenType.Multiply)
                left *= right;
            else if (op == TokenType.Divide)
                left /= right;
            return left;
        }
        static double PerformAddition(double left, TokenType op, double right)
        {
            if (op == TokenType.Add)
                left += right;

            if (op == TokenType.Subtract)
                left -= right;
            return left;
        }
        bool HasAdditionOperators() => tokens.Count > 0 && (tokens.Peek().Type == TokenType.Add || tokens.Peek().Type == TokenType.Subtract);
        bool HasMultiplicationOperator() => tokens.Count > 0 && (tokens.Peek().Type == TokenType.Multiply || tokens.Peek().Type == TokenType.Divide);
        public async Task RunAsync(string input)
        {
            var tokenizer = new Tokenizer(input);

            var tokens = new Stack<Token>();
            Token? nextToken;
            int attempts = 0;

            do
            {
                nextToken = tokenizer.GetNextToken();

                if (nextToken != null)
                    tokens.Push(nextToken);

                await Task.Delay(1);

                attempts++;
            }
            while (nextToken != null && attempts < 1000);

            PushTokensOntoStack(tokens);

            var val = Evaluate();

            if (val.HasValue)
                Interop.Log($"expr result : {val.Value}.");
            else Interop.Log("expr did not evaluate.");
            return;
        }
    }
    public class CSharpInterpreter : IInterpreter
    {
        public async Task RunAsync(string line)
        {
            ParseArguments(line, out string[] args, out _);
            line = ParseLoopParams(line, out string loop_param);
            
            var getCmdsTask = Interpreter.GetCommands();
            
            if (getCmdsTask is null)
                return;

            await getCmdsTask;

            if (getCmdsTask.Result == null)
                return;

            var commands = getCmdsTask.Result;

            foreach (Command command in commands)
                if (command.Equals(line))
                {
                    ExecuteCommand(args, command, loop_param.ToInt());
                    if (command.error != null)
                    {
                        Interop.Log(command.error);
                        command.error = null;
                        continue;
                    }
                    Command.Success(command.syntax);
                }
            return;
        }
        #region Grammar
        private const char Loop = '$';
        private const char EndLine = ';';
        private const char ArgumentsStart = '(';
        private const char ArgumentsEnd = ')';
        private const string ParameterSeperator = ", ";
        #endregion
        #region Parsing
        public static bool HasArgs(string input)
        {
            return input.Contains(ArgumentsStart) && input.Contains(ArgumentsEnd);
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
        private static void ExecuteCommand(string[] args, Command command, int loopCt)
        {
            for (int l = 0; l < loopCt; ++l)
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
                Interop.Log(e.Message);
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
        #endregion
    }
    public class Interpreter
    {
        static List<IInterpreter> ActiveInterpreters = new() { new CSharpInterpreter(), new ArithmeticInterpreter() };
        public Interpreter()
        {
        }
        public static void TryCallLine(string line)
        {
            foreach (var interpreter in ActiveInterpreters)
                interpreter.RunAsync(line);
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
        public static async Task<Command[]> GetCommands()
        {
            EditorEvent e = new(EditorEventFlags.GET_COMMAND_LIBRARY_C_SHARP);
            object? lib = null;
            e.action = (e) => { lib = e.First(); };
            Interop.RaiseInspectorEvent(e);
            float time = 0;

            while (!e.processed && time < 1500)
            {
                if (lib is Command[] commands)
                    return commands;

                time += 15f;
                await Task.Delay(15);
            }
            return null;
        }
    }
    public enum TokenType
    {
        VarDecl,
        Delete,
        
        LeftParen,
        RightParen,
        
        Assignment,
        Add,
        Subtract,

        Multiply,
        Divide,

        For,
        If,
        Return,
        Null,

        Number,
        Char,
        Identifier,
    }
    public interface IInterpreter
    {
        public abstract Task RunAsync(string input);
    }

}
