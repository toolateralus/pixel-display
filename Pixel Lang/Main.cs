




using Microsoft.VisualBasic;
using System.Numerics;
using System.Reflection;
using System.Text;
using static Interpreter;
// program begin


while (true)
{
    var err = Err();

    if (err != null)
        Console.Write($"error : {err}");

    string name = Console.ReadLine();
    
    if (name is null)
        continue;

    TryCallLine(name);
}





// program end

public class Token
{
    /// <summary>
    /// The type of the token, used for performing operations and executing expressions.
    /// </summary>
    public TokenType Type { get; }
    /// <summary>
    /// the string representation of the token
    /// </summary>
    public string Value { get; set; }
    public Token(TokenType type, string value)
    {
        this.Value = value;
        this.Type = type;
    }
    [Obsolete]
    public Token? ToValue()
    {
        foreach (var interpreter in (IEnumerable<TokenInterpreter>)(from intptr in Interpreter.ActiveInterpreters
                                                                    where intptr as TokenInterpreter != null
                                                                    let arith_intptr = intptr as TokenInterpreter
                                                                    select arith_intptr))
            if (interpreter.variables.ContainsKey(Value))
                return interpreter.variables[Value];
        return null;
    }
}
public class Function : Token
{
    #region Standard Functions
    static List<Function> Functions = new();
    static Function()
    {
        Functions = new()
            {
                print,


            };
    }

    static Function print = new((args) =>
    {
        string output = "";
        foreach (var arg in args)
        {
            output += arg.Value + " ";
        }
        Interop.Log(output);

    }, TokenType.NULL)
    {
        Value = "print",
    };


    #endregion
    public Action<List<Token>> function;
    public TokenType ReturnType = TokenType.NULL;
    public Function(Action<List<Token>> funct, TokenType returnType) : base(TokenType.FUNCTION, "funct")
    {
        function = funct;
        ReturnType = returnType;
        Functions.Add(this);
    }

    public virtual double? Invoke(List<Token> args)
    {
        if (function is null)
        {
            Interop.Log("Function not implemented.");
            return 1;
        }
        else
        {
            function.Invoke(args);
            return 0;
        }
    }
    public bool Equals(Token token)
    {
        if (token.Value == Value && token.Type == Type)
            return true;
        return false;
    }
    internal static double? Call(Token function, List<Token> arguments)
    {
        foreach (var funct in Functions)
            if (funct.Equals(function))
                return funct.Invoke(arguments);
        return 1;
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
                "var" => new Token(TokenType.VAR_DECL, identifier),
                "delete" => new Token(TokenType.DELETE, identifier),
                "for" => new Token(TokenType.FOR, identifier),
                "if" => new Token(TokenType.IF, identifier),
                "return" => new Token(TokenType.RETURN, identifier),
                "null" => new Token(TokenType.NULL, identifier),
                "funct" => new Token(TokenType.FUNCTION, identifier),
                _ => new Token(TokenType.IDENTIFIER, identifier),
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

            return new Token(TokenType.NUMBER, numberBuilder.ToString());
        }
        Token? parseOperators(char currentChar)
        {
            switch (currentChar)
            {
                case '+':
                    position++;
                    return new Token(TokenType.ADD, "+");
                case '-':
                    position++;
                    return new Token(TokenType.SUBTRACT, "-");
                case '=':
                    position++;
                    return new Token(TokenType.ASSIGN, "=");
                case '(':
                    position++;
                    return new Token(TokenType.LEFTPAREN, "(");
                case ')':
                    position++;
                    return new Token(TokenType.RIGHTPAREN, ")");
                case '*':
                    position++;
                    return new Token(TokenType.MULTIPLY, "*");
                case '/':
                    position++;
                    return new Token(TokenType.DIVIDE, "/");
                case '.':
                    return new Token(TokenType.OBJECT_ACCESSOR, ".");
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
public class Interpreter
{
    public class Interop
    {
        public static event Action<string?>? OutputStream;

        public static void Log(object? o)
        {
            if (OutputStream?.GetInvocationList().Length == 0)
                Console.WriteLine(o);
            else OutputStream?.Invoke(o?.ToString());
        }
    }
    public static bool SubscribeInterpreter(IInterpreter interpreter)
    {
        if (ActiveInterpreters.Contains(interpreter))
            return false;

        ActiveInterpreters.Add(interpreter);
        return true;
    }
    public static bool TryUnsubscribeInterpreter<T>() where T: IInterpreter
    {
        for (int i = 0; i < ActiveInterpreters.Count; i++)
        {
            IInterpreter? item = ActiveInterpreters[i];
            
            if (item != null && item.GetType() == typeof(T))
            {
                ActiveInterpreters.Remove(item);
                return true;
            }
        }
        return false; 
    }

    internal static List<IInterpreter> ActiveInterpreters = new() { new TokenInterpreter() };
    private static double? last_err;

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
    public static TokenFamily CheckFamily(TokenType type)
    {
        switch (type)
        {
            case TokenType.IDENTIFIER:
                return TokenFamily.IDENTIFIER;

            case TokenType.VAR_DECL:
            case TokenType.DELETE:
            case TokenType.FOR:
            case TokenType.IF:
            case TokenType.NULL:
            case TokenType.FUNCTION:
            case TokenType.RETURN:
                return TokenFamily.KEYWORD;


            case TokenType.ASSIGN:
            case TokenType.LEFTPAREN:
            case TokenType.RIGHTPAREN:
            case TokenType.ADD:
            case TokenType.SUBTRACT:
            case TokenType.MULTIPLY:
            case TokenType.DIVIDE:
            case TokenType.OBJECT_ACCESSOR:
                return TokenFamily.OPERATOR;

            case TokenType.NUMBER:
            case TokenType.CHAR:
                return TokenFamily.VALUE;

            default:
                return TokenFamily.UNDEFINED;
        };
    }
    public static void TryCallLine(string line)
    {
        foreach (var interpreter in ActiveInterpreters)
            interpreter.RunAsync(line);
    }
    internal static void PushErr(double? value)
    {
        if (value.HasValue)
            last_err = value.Value;
        else last_err = null;
    }
    internal static double? Err()
    {
        return last_err; 
    }
}
public static class ExtensionMethods
{
    public static Vector2 ToVector(this string input)
    {
        var split = input.Split(',');

        if (!split.Any())
            return default;

        string x = split.First();
        string y = split.Last();

        foreach (var _char in disallowed_chars)
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
    public static List<char> disallowed_chars = new()
    {
        ';',
        '\0',
        '(',
        ')',
        '"',
    };
}
public class TokenInterpreter : IInterpreter
{
    private Stack<Token> tokens = new();
    internal Dictionary<string, Token> variables = new();
    public void PushTokensOntoStack(Stack<Token> tokens)
    {
        this.tokens = new Stack<Token>(tokens);
    }
    public double? Evaluate()
    {
        var result = ParseExpression();
        Interpreter.PushErr(result);
        return result;
    }

    private double? ParseExpression()
    {
        var keyword = tokens.Peek();
        var type = keyword.Type;
        var family = CheckFamily(type);
        switch (family)
        {
            case TokenFamily.UNDEFINED:
                Interop.Log(keyword);
                break;

            case TokenFamily.KEYWORD:
                switch (type)
                {
                    case TokenType.VAR_DECL:
                        return PerformVariableDeclaration();
                    case TokenType.DELETE:
                        return PerformVariableDeletion();
                    case TokenType.FUNCTION:
                        return PerformFunctionExecution();
                    default:
                        break;
                }
                break;
            case TokenFamily.OPERATOR:
                break;
            case TokenFamily.VALUE:
                return ParseAddition();
            case TokenFamily.IDENTIFIER:
                return PerformObjectAccess();

        }
        return null;
    }
    private double? PerformFunctionExecution()
    {
        var keyword = tokens.Pop();
        var function = tokens.Pop();
        var functName = function.Value;

        if (function.Type != TokenType.IDENTIFIER)
            return null;

        // theres no way for the parser to know it's a function so we just rectify it here.
        function = new(TokenType.FUNCTION, functName);

        List<Token> arguments = new();

        // parsing function arguments
        while (tokens.Count > 0)
        {
            var token = tokens.Peek();
            var family = CheckFamily(token.Type);

            if (family == TokenFamily.IDENTIFIER)
            {
                if (variables.ContainsKey(token.Value))
                {
                    arguments.Add(variables[token.Value]);
                    tokens.Pop();
                    continue;
                }
            }

            switch (family)
            {
                case TokenFamily.UNDEFINED:
                case TokenFamily.KEYWORD:
                case TokenFamily.OPERATOR:
                    continue;

                case TokenFamily.VALUE:
                    token = tokens.Pop();
                    arguments.Add(token);
                    continue;
                case TokenFamily.IDENTIFIER:
                    token = tokens.Pop();
                    arguments.Add(token);
                    continue;
                default:
                    continue;
            }
        }

        return Function.Call(function, arguments);

    }
    private double? PerformVariableDeletion()
    {
        var keyword = tokens.Pop();
        var identifier = tokens.Pop();

        if (!variables.ContainsKey(identifier.Value))
        {
            Interop.Log("delete failed: variable not found.");
            return null;
        }

        variables.Remove(identifier.Value);
        Interop.Log("delete succeeded.");
        return null;
    }
    private double? PerformVariableAcess(Token identifier)
    {
        variables.TryGetValue(identifier.Value, out var value);

        var accessor = tokens.Pop();
        var @operator = accessor.Type;

        if (CheckFamily(@operator) != TokenFamily.OPERATOR)
            return null;

        switch (@operator)
        {
            case TokenType.OBJECT_ACCESSOR:
                return PerformObjectMemberAccess(value);
            case TokenType.ASSIGN:
                return PerformObjectOverwrite(identifier.Value);

                //TODO: implement math expressions between pre defined variables
        }
        return null;
    }
    private double? PerformObjectAccess()
    {
        var identifier = tokens.Pop();

        if (variables.ContainsKey(identifier.Value))
            return PerformVariableAcess(identifier);


        return null;
    }
    private double? PerformObjectOverwrite(string key)
    {
        var newVal = tokens.Pop();

        if (CheckFamily(newVal.Type) != TokenFamily.VALUE)
            return null;

        variables[key] = newVal;
        return null;
    }
    private double? PerformObjectMemberAccess(Token value)
    {
        var target = tokens.Pop();

        if (target.Type != TokenType.IDENTIFIER)
            return null;

        var assignment = tokens.Pop();

        if (assignment.Type != TokenType.ASSIGN)
            return null;

        var newValue = tokens.Pop();

        if (CheckFamily(newValue.Type) != TokenFamily.VALUE)
            return null;

        var type = value.GetType();
        var field = type.GetRuntimeField(target.Value);

        if (field != null)
        {
            Interop.Log($"{target} set to {newValue}!");
            field.SetValue(value, newValue);
            return null;
        }
        Interop.Log("Object access failure.");
        return null;
    }
    private double? PerformVariableDeclaration()
    {
        var keyword = tokens.Pop();

        var identifier = tokens.Pop();

        if (identifier.Type != TokenType.IDENTIFIER)
            return null;

        var assignment_operator = tokens.Pop();

        if (assignment_operator.Type != TokenType.ASSIGN)
            return null;

        var value = tokens.Pop();

        switch (CheckFamily(value.Type))
        {
            case TokenFamily.UNDEFINED:
            case TokenFamily.KEYWORD:
            case TokenFamily.OPERATOR:
                Interop.Log($"Invalid variable declaration. Cannot convert {value.Type} to {identifier.Type}");
                break;

            case TokenFamily.IDENTIFIER:
            case TokenFamily.VALUE:
                switch (value.Type)
                {
                    case TokenType.NULL:
                    case TokenType.NUMBER:
                    case TokenType.IDENTIFIER:
                    case TokenType.CHAR:
                        Interop.Log($"{value.Type} {identifier.Value} = {value.Value}");
                        variables.Add(identifier.Value, new(identifier.Type, value.Value));
                        break;
                }
                return null;

            default:
                break;
        }
        return null;
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

        if (token.Type == TokenType.SUBTRACT)
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
        bool isNumber = token.Type == TokenType.NUMBER;
        bool isLeftParentheses = token.Type == TokenType.LEFTPAREN;

        if (isNumber)
        {
            tokens.Pop();
            return double.Parse(token.Value);
        }

        if (isLeftParentheses)
        {
            tokens.Pop();
            var result = ParseExpression();
            bool missing_args = tokens.Count == 0 || token.Type != TokenType.RIGHTPAREN;

            if (missing_args)
                throw new Exception($"Missing closing parentheses");

            tokens.Pop();
            return result;
        }

        return null;
    }

    private static double PerformMultiplication(double left, TokenType op, double right)
    {
        if (op == TokenType.MULTIPLY)
            left *= right;
        else if (op == TokenType.DIVIDE)
            left /= right;
        return left;
    }
    private static double PerformAddition(double left, TokenType op, double right)
    {
        if (op == TokenType.ADD)
            left += right;

        if (op == TokenType.SUBTRACT)
            left -= right;
        return left;
    }

    bool HasAdditionOperators() => tokens.Count > 0 && (tokens.Peek().Type == TokenType.ADD || tokens.Peek().Type == TokenType.SUBTRACT);
    bool HasMultiplicationOperator() => tokens.Count > 0 && (tokens.Peek().Type == TokenType.MULTIPLY || tokens.Peek().Type == TokenType.DIVIDE);

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
            Interop.Log($"{val.Value}");

        return;
    }
}
public enum TokenFamily : long
{
    UNDEFINED = -long.MaxValue,
    KEYWORD = 0,
    OPERATOR = 1,
    VALUE = 2,
    IDENTIFIER = 4,
}
public enum TokenType
{
    VAR_DECL,
    DELETE,

    LEFTPAREN,
    RIGHTPAREN,

    ASSIGN,
    ADD,
    SUBTRACT,

    MULTIPLY,
    DIVIDE,

    FOR,
    IF,
    RETURN,
    NULL,

    NUMBER,
    CHAR,
    IDENTIFIER,
    OBJECT_ACCESSOR,
    FUNCTION,
}
public interface IInterpreter
{
    public abstract Task RunAsync(string input);
}



