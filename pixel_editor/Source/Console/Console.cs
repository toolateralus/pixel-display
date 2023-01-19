using pixel_renderer;
using pixel_editor;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

namespace pixel_editor
{
    public class Console
    {
        public static Console Current 
        {
            get
            {
                if (_console == null)
                {

                    // init singleton and use it to fill cmd list; 
                    _console = new();
                    var type = _console.GetType();
                    var methods = type.GetRuntimeMethods();
                    foreach (var method in methods)
                        if (method.ReturnType == typeof(Command) && method.Name.Contains("cmd_"))
                        {
                            Command? item = (Command)method.Invoke(null, null);
                            if (item != null && !Current.Active.Contains(item))
                                Current.Active.Add(item);
                        }
                }

                return _console; 
            }
            set => _console = value; 
        }

        private const string divider = "\n-- -- --\n";
        private static Console _console; 

        public static void Print(object? o, bool includeDateTime = false)
        {
            var msg = o.ToString();
            var e = new EditorEvent(msg, includeDateTime);
            Editor.QueueEvent(e); 
        }
        public static void Error(object? o = null, int? textColorAlterationDuration = null)
        {
            string? msg = o.ToString();
            EditorEvent e = new(msg, true);
           
                if (textColorAlterationDuration is not null)
                    e.action = RedTextForMsAsync( (int)textColorAlterationDuration);
            Editor.QueueEvent(e);
        }

        public static Action<object[]?> RedTextForMsAsync(int delay)
        {
            return async (o) =>
            {
                Editor.Current.RedText().Invoke(o);
                await Task.Delay(delay * 1000);
                Editor.Current.BlackText().Invoke(o);
            };
        }
        public static void Clear()
        {
            EditorEvent editorEvent = new EditorEvent("");
            editorEvent.ClearConsole = true;
            Editor.QueueEvent(editorEvent);

            Error("Console Cleared", 1);
        }
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

        public static Command cmd_set_stage()
        {
            return new()
            {
                phrase = "stage.Set;",
                action = async (o) =>
                {
                    string stageName = (string)o[0];
                    bool loadAsynchronously = false;

                    if (o.Length > 1 && o[1] is bool)
                        loadAsynchronously = (bool)o[1];

                    Project project = Runtime.Instance.LoadedProject;

                    if (project is null) return;

                    var stage = Project.GetStageByName(stageName);
                    
                    if (stage == null)
                        Runtime.Log($"Stage load cancelled : Stage {stageName} not found.");

                    var prompt = PromptAsync($"{stage.Name} Found. Do you want to load this stage?");
                    await prompt;
                  
                    switch (prompt.Result)
                    {
                        case PromptResult.Yes:
                            Print($"Stage {stage.Name} set.");
                            Runtime.Instance.SetStageAsset(stage);
                            break;
                        case PromptResult.No:
                            Print("Stage not set.");
                            break;
                        case PromptResult.Cancel:
                            Print("Set Stage cancelled.");
                            break;
                        case PromptResult.Timeout:
                            Print("Set Stage timed out.");
                            break;
                        default:
                            break;
                    }
                },
                description = "Attempts to find a stage by name, and if found, prompts the user to load it or not."
            };
        }
        public static Command cmd_load_project()
        {
            return new()
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
                                                $"Do you want to load this project?";

                        Task<PromptResult> result = PromptAsync(question, 60f);

                        await result;

                        switch (result.Result)
                        {
                            case PromptResult.Yes:
                                Console.Print($"Project {name} set.");
                                Runtime.Instance.SetProject(project);
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
        }
        public static Command cmd_reload_stage()
        {
            return new()
            {
                phrase = "reload;|/r;|++r;",
                action = (o) =>
                {
                    Runtime.Instance.ResetCurrentStage();
                },
                args = null,
                description = "Reloads the currently loaded stage",
            };
        }
        public static Command cmd_get_node()
        {
            return new()
            {
                phrase = "node.Get;",
                action = (e) =>
                {
                    string name = (string)e[0];
                    Node node = Runtime.Instance.GetStage().FindNode(name);
                    Editor.Current.Inspector.DeselectNode();
                    Editor.Current.Inspector.SelectNode(node);
                    if (node is not null)
                    {
                        PrintNodeInformation(node);
                        return;
                    }
                    Print($"getNode({name}) \n Node with name {name} not found.");
                },
                args = null,
                description = "Retrieves the node of name specified",
            };
        }
        public static Command cmd_list_node()
        {
            return new()
            {
                phrase = "node.List;",
                action = ListNodes,
                args = null,
                description = "Lists all nodes in currently loaded stage.",
            };

        }
        public static Command cmd_set_node_field()
        {
            return new()
            {
                phrase = "node.Set;",
                args = Array.Empty<object>(),
                action = SetNodeField,
                description = "neccesary arguments : (string Name, string FieldName, object value) " +
            "\n gets a node and attempts to write the provided value to specified field.",
            };
        }
        public static Command cmd_call_node_method()
        {
            return new()
            {
                phrase = "node.Call;",
                args = Array.Empty<object>(),
                action = CallNodeMethod,
                description = "neccesary arguments : (string Name, string MethodName) {Method must be paramaterless.} " +
                             "\n Gets a node by Name, finds the provided method by MethodName, and invokes the method.",
            };
        }
        public static Command cmd_set_resolution()
        {
            return new()
            {
                phrase = "resolution.Set;",
                action = (e) =>
                {
                    Vec2 vector = (Vec2)e[0];
                    Vec2Int newRes = (Vec2Int)vector;
                    Console.Print(vector.AsString());
                    Runtime.Instance.renderHost.GetRenderer().Resolution = newRes;
                },
                description = "sets the resolution to the specified Vec2. \n syntax : resolution.Set(vec:x,y);"
            };

        }
        public static Command cmd_get_resolution()
        {
            return new()
            {
                phrase = "resolution.Get;",
                action = (e) =>
                {
                    Console.Print(((Vec2)Runtime.Instance.renderHost.GetRenderer().Resolution).AsString());
                },
                description = "gets the resolution and prints it to the console"
            };
        }
        public static Command cmd_clear_console()
        {
            return new()
            {
                phrase = "cclear;",
                action = (e) => Clear(),
                description = "Clears the console's output",
            };
        }
        public static Command cmd_spawn_generic()
        {
            return new()
            {
                phrase = "++n;|newNode;",
                action = (o) => Runtime.Instance.GetStage().create_generic_node(),
                description = "Spawns a generic node with a Rigidbody , Sprite, and Collider, and adds it to the current Stage."
            };

        }
        public static Command cmd_help()
        {
            return new()
            {
                phrase = "help;|help|Help|/h",
                action = (o) =>
                {
                    string output = "";
                    foreach (var cmd in Current.Active)
                        output += divider + cmd.phrase + "\n" + cmd.description + divider;
                    Print(output);
                },
            };
        }
        public static Command cmd_log()
        {
            return new()
            {
                phrase = "log;",
                action = (o) => Console.Print(o[0]),
                description = "Logs a message to the console, Some characters will cause this to fail."
            };
        }
        public static Command cmd_set_camera()
        {
            return new()
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
                description = "\n {must not be a property or method} Sets Field in camera by name on node of provided name \n syntax : cam(str:<nodeName>, str:<fieldName>, object:value)",
            };
        }

        private static void PrintNodeInformation(Node node)
        {
            Print(
                $"Node Found! " +
                $"\n Name : {node.Name} " +
                $"\n Position : {node.position.AsString()} " +
                $"\n UUID : {node.UUID} " +
                $"\n Tag: {node.tag} " +
                $"\n Component Count : {node.ComponentsList.Count}");
        }

        private static void ListNodes(params object[]? e)
        {
            string nodesList = "";
            char nonBreakingspace = '\u2007';
            foreach (Node node in Runtime.Instance.GetStage().Nodes)
                nodesList += node.Name.PadLeft(16, nonBreakingspace) + " ";
            Console.Print($"{nodesList}");
        }
        private static void SetNodeField(params object[]? e)
        {
            if (e.Length >= 3)
            {
                string nName = (string)e[0];
                string fName = (string)e[1];
                object value = e[2];

                Node? node = Runtime.Instance.GetStage().FindNode(nName);
                Type type = node.GetType();
                FieldInfo? field = type.GetRuntimeField(fName);
                field?.SetValue(node, value);
            }
        }
        private static void CallNodeMethod(params object[]? e)
        {
            if (e is not null && e.Length > 1)
            {
                string nName = (string)e[0];
                string fName = (string)e[1];

                if (Runtime.Instance.GetStage().FindNode(nName) is not Node node)
                {
                    Error($"\"{nName}.{fName}()\" Node not found!", 2);
                    return;
                }

                Type type = node.GetType();
                MethodInfo method = type.GetMethod(fName);
                if (method is null)
                {
                    Error($"\"{nName}.{fName}()\" Method not found!", 2);
                    return;
                }

                method.Invoke(node, null);
                Print($"\"{nName}.{fName}()\" Call succesful.");
            }
        }

        public List<Command> Active = new();


    }

}
