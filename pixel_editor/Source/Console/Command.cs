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
    public class Command
    {
        public string phrase = "";
        public string description = "";
        public Action<object[]?>? action;
        public object[]? args;

        internal static void Call(string line) => CommandParser.TryCallLine(line, Console.Current.Active);
        public void Invoke() => action?.Invoke(args);
        public bool Equals(string input)
        {
            string withoutArgs = CommandParser.ParseArguments(input, out _);
            withoutArgs = CommandParser.ParseLoopParams(withoutArgs, out _);

            string[] split = phrase.Split('|');

            foreach (var line in split)
                if (line.Equals(withoutArgs))
                    return true;
            return false;
        }

    }
}

