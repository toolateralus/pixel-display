using System;
using System.Linq;
using pixel_renderer;

namespace pixel_editor
{
    public class Command
    {
        public static Command reload_stage = new()
        {
            phrase = "reload|Reload|realod|/r",
            action = (o) =>
            {
                Console.Print("Josh Is Cool!");
                Runtime.Instance.ResetCurrentStage();
            },
            args = null
        };
        public static Command spawn_generic = new()
        {
            phrase = "genericNode|spawnGeneric|/sgn|++n",
            action = (o) => Runtime.Instance.GetStage().create_generic_node(),
            args = null
        };
        public static readonly Command[] Active = new Command[]
        {
            reload_stage,
            spawn_generic,
        };
        public string phrase = "";
        public Action<object[]?>? action;
        public object[]? args;
        public bool Equals(string input)
        {
            string newPhrase = "";

            if (input.Contains('$'))
                newPhrase = input.Split('$')[0];
            else newPhrase = input;

            if (input.Contains('('))
            {
                int indexOfStart = input.IndexOf('(');
                int indexOfEnd = input.IndexOf(')');
                string chunk = "";
                for (int i = indexOfStart; i < indexOfEnd; ++i)
                    chunk += input[i];
            }

                var split = phrase.Split('|');

            foreach (var line in split)
                if (line.Equals(newPhrase))
                    return true;
            return false; 
        }

        public void Execute()
        {
            action?.Invoke(args);
        }

        internal static void Call(string line)
        {
            foreach (var command in Active)
                if (command.Equals(line))
                {
                    int count = 0;
                    
                    if (line.Contains('$'))
                        count = line.ToInt();

                    if (count == 0)
                    {
                        command.Execute();
                        return; 
                    }
                    for(int i = 0; i < count; ++i)
                        command.Execute();
                }
        }
    }
 
}

