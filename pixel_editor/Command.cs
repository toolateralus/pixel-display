using System;
using System.Linq;
using System.Windows;
using pixel_renderer;

namespace pixel_editor
{
    public class Command
    {
        private static Command reload_stage = new()
        {
            phrase = "reload;|/r;|++r;",
            action = (o) =>
            {
                Runtime.Instance.ResetCurrentStage();
            },
            args = null
        };
        private static Command spawn_generic = new()
        {
            phrase = "++n;|newNode;",
            action = (o) => Runtime.Instance.GetStage().create_generic_node(),
            args = null
        };
        private static Command get_node = new()
        {
            phrase = "getNode;",
            action = (e) => 
            {
                string name = (string)e[0];
                Node node = Runtime.Instance.GetStage().FindNode(name);
                Editor.Current.Inspector.SelectNode(node);
                if (node is not null)
                {
                    Console.Print(
                        $"\n Node Found! \n Name : { node.Name} \n Position : x : {node.position.x} y : {node.position.y} \n UUID : {node.UUID} \n Tag: {node.tag} \n Component Count : {node.ComponentsList.Count}");
                }
            },
            args = null,
        };
        public static readonly Command[] Active = new Command[]
        {
            get_node, 
            reload_stage,
            spawn_generic,
        };
        public string phrase = "";
        public Action<object[]?>? action;
        public object[]? args;

        public bool Equals(string input)
        {
            
            string withoutArgs = ParseParameters(input, out _);  
            withoutArgs = ParseIterator(withoutArgs, out _);

            string[] split = phrase.Split('|');

            foreach (var line in split)
                if (line.Equals(withoutArgs))
                    return true;
            return false;
        }
        private static string ParseIterator(string input, out string repeaterArgs)
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
        private static string ParseParameters(string input, out string parenthesesArgs)
        {
            parenthesesArgs = "";
            string withoutArgs = "";
            bool hasPArgs = input.Contains('(') && input.Contains(')'); 
            if (hasPArgs)
            {
                int indexOfStart = input.IndexOf('(');
                int indexOfEnd = input.IndexOf(';');

                for (int i = indexOfStart; i < indexOfEnd; ++i)
                    parenthesesArgs += input[i];

                if(parenthesesArgs.Length > 0)
                    withoutArgs = input.Replace(parenthesesArgs, "");
            }
            return withoutArgs;
        }
        public void Execute()
        {
            action?.Invoke(args);
        }
        internal static void Call(string line)
        {
            foreach (var command in Active)
                if (command.Equals(line))
                    TryParseLine(line, command);
        }
        private static void TryParseLine(string line, Command command)
        {
            string withoutArgs = ParseParameters(line, out string pArgs);
            withoutArgs = ParseIterator(line, out string rArgs);
            int count = rArgs.ToInt();

            // single execution paramaterless command; 
            if (count == 0 && pArgs.Length == 0)
            {
                command.Execute();
                return;
            }
            // command with params; 
            if (pArgs.Length > 0)
            {
                string args = (string)CommandArgsParser.Parse<string>(pArgs);
                command.args = new object[] { args };
                command.Execute(); 
            }
            for (int i = 0; i < count; ++i)  command.Execute();
        }
    }
 
}

