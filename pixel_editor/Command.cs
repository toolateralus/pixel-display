using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using pixel_renderer;
using pixel_renderer.FileIO;

namespace pixel_editor
{
    public enum PromptResult { Yes, No, Ok, Cancel, Timeout};
    public class Command
    {
        private static Command reload_stage = new()
        {
            phrase = "reload;|/r;|++r;",
            action = (o) =>
            {
                Runtime.Instance.ResetCurrentStage();
            },
            args = null,
            argsTypes = null,
        };
        private static Command spawn_generic = new()
        {
            phrase = "++n;|newNode;",
            action = (o) => Runtime.Instance.GetStage().create_generic_node(),
            args = null,
            argsTypes = null,

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
            argsTypes = new Type[] { typeof(string) },
        };
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
            argsTypes = new Type[] { typeof(string) },

        };
        private static Command set_node_field = new()
        {
            phrase = "setNode;",
            args = new object[] {  },

            argsTypes = new Type[] 
            { 
                typeof(string),
                typeof(string),
                typeof(object)
            },

            action = (e) => 
            {
                var fName = (string)e[0];
                var value = e[1];
            
            },
        };

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

        internal static void Call(string line)
        {
            foreach (var command in Active)
                if (command.Equals(line))
                    CommandParser.TryParseLine(line, command);
        }

        public void Invoke()
        {
            action?.Invoke(args);
        }
        public bool Equals(string input)
        {
            
            string withoutArgs = CommandParser.ParseArguments(input, out _);  
            withoutArgs = CommandParser.ParseIterator(withoutArgs, out _);

            string[] split = phrase.Split('|');

            foreach (var line in split)
                if (line.Equals(withoutArgs))
                    return true;
            return false;
        }

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
        public Type[]? argsTypes; 

    }

}

