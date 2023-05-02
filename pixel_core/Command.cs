using Pixel;
using System;

namespace Pixel.Types
{
    public enum PromptResult { Yes, No, Ok, Cancel, Timeout};
    public partial class Command
    {
        public const char PhraseVariantSeperator = '|';
        /// <summary>
        /// the way the command must be typed to be successfully interpreted. note: if your command looks like this -> node.List(); then you need to type this as node.List;
        /// </summary>
        public string phrase = "";
        /// <summary>
        /// A short summary of the function and usage of the command.
        /// </summary>
        public string description = "Please add a description to this command.";
        /// <summary>
        /// Syntactical guidelines for using the command properly, etc default args or explanation of syntax.
        /// </summary>
        public string syntax = "Please add syntactical guidelines to this command. e.g. node.Get(str:{{target}});";
        /// <summary>
        /// declaring members in this list will flag your command to have x arguments of n type, see CommandParser.type_identifiers {todo: make some documentation for this as its a private protected array}.
        /// </summary>
        public string[]? argumentTypes = null; 
        /// <summary>
        /// this is only not null when there has been an error, and will likely contain a string of information about the error.
        /// </summary>
        public object? error = null;
        /// <summary>
        /// this is what is invoked when the command is successfully called, and it is invoked under the parameters of this.args as needed.
        /// </summary>
        public Action<object[]?>? action;
        /// <summary>
        /// use this as a way to pass parameters into a command
        /// </summary>
        public object[]? args;
        public void Invoke() => action?.Invoke(args);
        public bool Equals(string input)
        {
            CSharpInterpreter.ParseArguments(input, out _, out var withoutArgs);
            withoutArgs = CSharpInterpreter.ParseLoopParams(withoutArgs, out _);

            string[] split = phrase.Split(PhraseVariantSeperator);

            foreach (var line in split)
                if (line.Equals(withoutArgs))
                    return true;
            return false;
        }
        public static void Success(string syntax) => Interop.Log($"{syntax} call successful");
    }
}

