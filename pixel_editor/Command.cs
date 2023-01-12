using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using pixel_renderer;
using pixel_renderer.FileIO;

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
                        $"Node Found! " +
                        $"\n Name : { node.Name} " +
                        $"\n Position : x : {node.position.x} y : {node.position.y} " +
                        $"\n UUID : {node.UUID} " +
                        $"\n Tag: {node.tag} " +
                        $"\n Component Count : {node.ComponentsList.Count}");
                    return; 
                }
                Console.Print($"getNode({name}) \n Node with name {name} not found."); 
            },
            args = null,
        };
        private static Command draw
        {

        }

        enum PromptResult { Yes, No, Ok, Cancel, Timeout};
        private static async Task<PromptResult> YesNoPromptAsync(string question, float? waitDuration = 60f)
        {
            Console.Print(question);
            for (int i = 0; i < waitDuration * 100; i++)
            {
                if (i % 100 == 0)
                {
                    int seconds = 10 * (5000 - i) / 1000;
                    Console.Print($"[Y/N/End] {seconds} seconds remaining");
                }

                if (Keyboard.IsKeyDown(Key.Y)) 
                    return PromptResult.Yes;
                if (Keyboard.IsKeyDown(Key.N))
                    return PromptResult.No; 
                if (Keyboard.IsKeyDown(Key.End))
                    return PromptResult.Cancel;

                await Task.Delay(10);
            };
            return PromptResult.Timeout; 
        }
        private static Command load_project = new()
        {

            phrase = "loadProject;",
            action = async (e) =>
            {
                string name = (string)e[0];
                var project = ProjectIO.ReadProject(name);

                if (project is not null)
                {
                    string question = $"Project Found! " +
                                            $"\n Name : {project.Name} " +
                                            $"\n UUID : {project.stageIndex} " +
                                            $"Do you want to load this project?";

                    Task<PromptResult> result = YesNoPromptAsync(question, 60f);
                    await result;
                    switch (result.Result)
                    {
                        case PromptResult.Yes:
                            Console.Print($"Project {name} set.");
                            break;
                        case PromptResult.No:
                            Console.Print("Project not set.");
                            break;
                        case PromptResult.Cancel:
                            Console.Print("Load Project cancelled.");
                            break; 
                        case PromptResult.Timeout:
                            Console.Print("Load Project timed out.");
                            break;
                        default:
                            break;
                    }

                }
            },
            args = new object[] { },

        };


        public static readonly Command[] Active = new Command[]
        {
            load_project,
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

                command.args = new object[] 
                {
                    args
                };
                command.Execute(); 
            }
            for (int i = 0; i < count; ++i)  command.Execute();
        }
    }

}

