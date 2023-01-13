using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
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

        internal static void Call(string line) => CommandParser.TryCallLine(line, Active);
        private static async Task<PromptResult> PromptAsync(string question, float? waitDuration = 60f)
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
        public void Invoke()
        {
            action?.Invoke(args);
        }
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

        private static Command cmd_load_project = new()
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

                    Task<PromptResult> result = PromptAsync(question, 60f);
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
            description = "Loads a project @../Pixel/Projects of specified name, and if found, prompts the user to load the project as the current project."
        };
        private static Command cmd_reload_stage = new()
        {
            phrase = "reload;|/r;|++r;",
            action = (o) =>
            {
                Runtime.Instance.ResetCurrentStage();
            },
            args = null,
            description = "Reloads the currently loaded stage",
        };
        private static Command cmd_get_node = new()
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
                        $"\n Name : {node.Name} " +
                        $"\n Position : x : {node.position.x} y : {node.position.y} " +
                        $"\n UUID : {node.UUID} " +
                        $"\n Tag: {node.tag} " +
                        $"\n Component Count : {node.ComponentsList.Count}");
                    return;
                }
                Console.Print($"getNode({name}) \n Node with name {name} not found.");
            },
            args = null,
            description = "Retrieves the node of name specified",

        };
        private static Command cmd_set_node_field = new()
        {
            phrase = "getNode.Set;",
            args = Array.Empty<object>(),
            action = (e) =>
            {
                if (e.Length < 3)
                    return;
                string nName = (string)e[0];
                string fName = (string)e[1];
                object value = e[2];

                Node? node = Runtime.Instance.GetStage().FindNode(nName);
                Type type = node.GetType();
                FieldInfo? field = type.GetRuntimeField(fName);
                field.SetValue(node, value);
            },
            description = "neccesary arguments : (string Name, string FieldName, object value) " +
            "\n gets a node and attempts to write the provided value to specified field.",
        };
        private static Command cmd_spawn_generic = new()
        {
            phrase = "++n;|newNode;",
            action = (o) => Runtime.Instance.GetStage().create_generic_node(),
            args = null,
            description = "Spawns a generic node with a Rigidbody and Sprite and adds it to the current Stage."

        };
        private static Command cmd_help = new()
        {
            phrase = "help;|help|Help|/h",
            action = (o) =>
            {
                string output = "";
                foreach (var cmd in Active)
                    output += cmd.phrase + "\n" + cmd.description + "\n\n";
                Console.Print(output);
            },
            args = null,
            description = "Spawns a generic node with a Rigidbody and Sprite and adds it to the current Stage."
        };
        private static Command cmd_log = new()
        {
            phrase = "log;",
            action = (o) => Console.Print(o[0]),
            args = null,
            description = "Logs a message to the console, Some characters will cause this to fail."
        };
        private static Command cmd_set_camera = new()
        {
            phrase = "cam;",
            args = Array.Empty<object>(),
            action = (e) =>
            {
                if (e.Length < 3)
                    return;

                string nName = (string)e[0];
                string fName = (string)e[1];
                object value = e[2];

                Node? node = Runtime.Instance.GetStage().FindNode(nName);
                Camera cam = node.GetComponent<Camera>();
                Type type = cam.GetType();
                FieldInfo? field = type.GetRuntimeField(fName);
                field.SetValue(cam, value);
            },
            description = "\n sets provided field in first camera",
        };

        public static Command[] Active { get; } = new Command[]
        {
            cmd_help,
            cmd_load_project,
            cmd_reload_stage,
            cmd_get_node,
            cmd_set_node_field,
            cmd_spawn_generic,
            cmd_log,
            cmd_set_camera,
        };
    }

}

