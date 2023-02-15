using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using pixel_renderer;

namespace pixel_editor
{
    public enum PromptResult { Yes, No, Ok, Cancel, Timeout};
    public partial class Command
    {
        /// <summary>
        /// This is the keyword used to invoke the command
        /// </summary>
        public string phrase = "";
        public string description = "Please add a description to this command.";
        public string syntax = "Please add syntactical guidelines to this command. e.g. \"node.Get(str:{{target}});\"";

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

        internal static void Call(string line) => CommandParser.TryCallLine(line, Console.Current.Active);
        public void Invoke() => action?.Invoke(args);
        public bool Equals(string input)
        {
            // TODO: Each command gets parsed twice, once for comparison and once for usage, instead we could parse the command once and if comparison true, return executable
            string withoutArgs = CommandParser.ParseArguments(input, out _);
            withoutArgs = CommandParser.ParseLoopParams(withoutArgs, out _);

            string[] split = phrase.Split('|');

            foreach (var line in split)
                if (line.Equals(withoutArgs))
                    return true;
            return false;
        }
        internal static void Success(string syntax) => Runtime.Log($"Invocation of {syntax} successful!");
    }
}

