using pixel_renderer;
using pixel_editor;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using pixel_renderer.Scripts;

namespace pixel_editor
{
  

    public class Console
    {

        #region General Commands
        public static Command cmd_help() => new()
        {
            phrase = "help;|help|Help|/h",
            syntax = "help();",
            action = (o) =>
            {
                string output = "";
                foreach (var cmd in Current.Active)
                    output += "\n" + cmd.syntax + "\n" + cmd.description + "\n" + divider;
                Print(output);
            },
        };
        public static Command cmd_clear_console() => new()
        {
            phrase = "cclear;",
            syntax = "cclear();",
            action = (e) =>
            {
                if (e is not null && e[0] is bool color)
                    Clear(color);
                else Clear();
            },
            description = "Clears the console's output",
        };
        public static Command cmd_log() => new()
        {
            phrase = "log;",
            syntax = "log(message);",
            argumentTypes = new string[] { "str:" },
            action = (o) => Console.Print(o[0]),
            description = "Logs a message to the console, Some characters will cause this to fail."
        };
        public static Command cmd_swap_theme() => new()
        {
            phrase = "theme;",
            description = "Swaps the current theme [DOES NOT WORK CURRENTLY]",
            syntax = "theme(light_or_dark);",
            argumentTypes = new string[] { "str:" },
            action = (o) =>
            {
                bool hasArg = o != null && o.Length > 0;
                if (hasArg)
                {
                    if (o[0] is string arg)
                    {
                        Editor.Current.SetTheme(arg);
                        return;
                    }
                }
            },
            args = null,
        };
        #endregion

        #region Renderer Commands
        public static Command cmd_set_resolution() => new()
        {
            phrase = "resolution.Set;",
            syntax = "resolution.Set(1024,1024);",
            argumentTypes = new string[] { "vec:" },
            action = (e) =>
            {
                Vec2 vector = (Vec2)e[0];
                Vec2Int newRes = (Vec2Int)vector;
                Console.Print(vector.AsString());
                Runtime.Instance.renderHost.GetRenderer().Resolution = newRes;
            },
            description = "sets the resolution to the specified Vec2"
        };
        public static Command cmd_get_resolution() => new()
        {
            phrase = "resolution.Get;",
            syntax = "resolution.Get();",
            action = (e) =>
            {
                Console.Print(((Vec2)Runtime.Instance.renderHost.GetRenderer().Resolution).AsString());
            },
            description = "gets the resolution and prints it to the console"
        };
        #endregion

        #region Node Commands
        public static Command cmd_set_node_field() => new()
        {
            phrase = "node.Set;",
            syntax = "node.Set(nodeName, fieldName, value);",
            argumentTypes = new string[] { "str:", "str:", "bool:" },
            args = null,
            action = SetNodeField,
            description = "gets a node and attempts to write the provided value to specified field.",

        };
        public static Command cmd_get_node() => new()
        {
            phrase = "node.Get;",
            syntax = "node.Get(nodeName);",
            argumentTypes = new string[] { "str:" },
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
        public static Command cmd_list_node() => new()
        {
            phrase = "node.List;",
            syntax = "node.List();",
            action = ListNodes,
            args = null,
            description = "Lists all nodes in currently loaded stage.",
        };
        public static Command cmd_call_node_method() => new()
        {
            phrase = "node.Call;",
            syntax = "node.Call(nodeName, methodName);",
            argumentTypes = new string[] { "str:", "str:" },
            action = CallNodeMethod,
            args = null,
            description = "Gets a node by Name, finds the provided method by MethodName, and invokes the method.",

        };
        public static Command cmd_spawn_generic() => new()
        {
            phrase = "++n;",
            syntax = "++n();",
            action = (o) => Runtime.Instance.GetStage().AddNode(Rigidbody.Standard()),
            description = "Spawns a generic node with a Rigidbody , Sprite, and Collider, and adds it to the current Stage."
        };
        public static Command cmd_add_child() => new()
        {
            phrase = "node.Child;",
            syntax = "node.Child(string parentName);",
            description = "Adds a child node underneath the target parent if found.",
            argumentTypes = new string[] { "str:" },
            action = AddChild,
        };
        public static Command cmd_move_node() => new()
        {
            phrase = "node.Move;",
            syntax = "node.Move(string nodeName, Vec2 destination);",
            description = "Sets the node's position to the provided vector.",
            argumentTypes = new string[] { "str:", "vec:" },
            action = (e) =>
            {
                if (!TryGetNodeByNameAtIndex(e, out Node node, 0)) return;
                if (!TryGetArgAtIndex(1, out Vec2 vec, e)) return;
                node.Move(vec);
            },
        };
        public static Command cmd_remove_component() => new()
        {
            phrase = "node.RemoveComponent;",
            syntax = "node.RemoveComponent(string nodeName, string componentType);",
            description = "Removes one componenet from node of specified type.",
            argumentTypes = new string[] { "str:", "str:" },
            action = (e) =>
            {
                if (!TryGetNodeByNameAtIndex(e, out Node node, 0)) return;
                if (!TryGetArgAtIndex(1, out string type, e)) return;

                if (Type.GetType(type) is not Type t)
                {
                    Command.Error("node.RemoveComponent(string nodeName, string componentType);", CmdError.NullReference);
                    return;
                }
                foreach (var comp in node.ComponentsList)
                    if (comp.GetType() == t)
                    {
                        node.RemoveComponent(comp);
                        Print($"{nameof(t)} added to {node.Name}.");
                    }
            },
        };
        #endregion

        #region Asset/Project/Stage/IO Commands
        public static Command cmd_set_camera() => new()
        {
            phrase = "cam;",
            syntax = "cam(Name, Field, float Value);",
            argumentTypes = new string[] { "str:", "str:", "float:" },
            action = (e) =>
            {
                if (e.Length < 3)
                    return;

                string nName = (string)e[0];
                string fName = (string)e[1];
                object value = e[2];

                Node? node = Runtime.Instance.GetStage().FindNode(nName);
                if (node is null)
                    Console.Print("Node was not found.");
                Camera cam = node.GetComponent<Camera>();
                Type type = cam.GetType();
                FieldInfo? field = type.GetRuntimeField(fName);
                field.SetValue(cam, value);
            },
            description = "\n {must not be a property or method} Sets Field in camera by name on node of provided name \n syntax : cam(str:<nodeName>, str:<fieldName>, object:value)",
        };
        public static Command cmd_random_background() => new()
        {
            phrase = "stage.Background.Randomize;",
            syntax = "stage.Background.Randomize();",
            action = RandomizeBackground,
            description = "Sets the current stage's background to a random array of colors until reloaded.",
        };
        public static Command cmd_asset_exists() => new()
        {
            description = "Shows a count of all loaded assets, and an option to see more info.",
            syntax = "fetch(assetName);",
            argumentTypes = new string[] { "str:" },
            phrase = "fetch;",
            action = AssetExists,
        };
        public static Command cmd_load_project() => new()
        {
            phrase = "loadProject;",
            syntax = "loadProject(projectName);",
            argumentTypes = new string[] { "str:" },
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
        public static Command cmd_set_stage() => new()
        {
            phrase = "stage.Set;",
            syntax = "stage.Set(stageName);",
            argumentTypes = new string[] { "str:" },
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
                {
                    Runtime.Log($"Stage load cancelled : Stage {stageName} not found.");
                    return;
                }

                var prompt = PromptAsync($"{stage.Name} Found. Do you want to load this stage?");
                await prompt;

                switch (prompt.Result)
                {
                    case PromptResult.Yes:
                        Print($"Stage {stage.Name} set.");
                        Runtime.Instance.SetStage(stage);
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
            description = "[STAGE MUST BE IN PROJECT] \nAttempts to find a stage by name, and if found, prompts the user to load it or not."
        };
        public static Command cmd_reload_stage() => new()
        {
            phrase = "reload;",
            syntax = "reload(int stageIndex);",
            description = "reloads the currently loaded stage",
            argumentTypes = new string[] { "int:" },
            action = (o) =>
            {
                if (!TryGetArgAtIndex<int>(0, out int index, o)) return;
                Runtime.TryLoadStageFromProject(index);
            },

        };
        public static Command cmd_list_stages() => new()
        {
            phrase = "stages.List;",
            description = "Lists the names of the stage metadata loaded for the current project",
            syntax = "stages.List(inst || file); \n \n{must use either inst or file to get a result, \n inst == instanced/loaded stages | \n file == loaded metadata for stage files in project.}",
            argumentTypes = new string[] { "str:" },
            action = (o) =>
            {

                if (!TryGetArgAtIndex<string>(0, out string stageListingType, o))
                    return;
                ListStages(stageListingType);
            },
            args = null,
        };
        #endregion
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
                    e.action = RedTextAsync( (int)textColorAlterationDuration);
            Editor.QueueEvent(e);
        }
        internal static async Task<PromptResult> PromptAsync(string question, float? waitDuration = 60f)
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
        public static void Clear(bool randomColor = false)
        {
            EditorEvent editorEvent = new EditorEvent("");
            editorEvent.ClearConsole = true;
            Editor.QueueEvent(editorEvent);
            
            if(randomColor)
                Error("Console Cleared", 1);
        }

        public static Action<object[]?> RedTextAsync(int delay)
        {
            return async (o) =>
            {
                Editor.Current.RedText().Invoke(o);
                await Task.Delay(delay * 1000);
                Editor.Current.BlackText().Invoke(o);
            };
        }

        public static bool TryGetArgAtIndex<T>(int index, out T arg, object[] o)
        {
            bool hasArg = o != null && o.Length > index;

            if (hasArg && o[index] is T val)
            {
                arg = val;
                return true; 
            }
            arg = (T)Convert.ChangeType(null, typeof(T));
            return false ; 
        }

        static void PrintLoadedStageNames()
        {
            foreach (var stage in Runtime.Instance.LoadedProject.stages)
                Console.Print(stage.Name);
        }
        static void PrintLoadedStageMetaFileNames()
        {
            foreach (var stage in Runtime.Instance.LoadedProject.stagesMeta)
                Console.Print(stage?.Name ?? "null");
        }
        static void ListStages(string s)
        {
            switch (s)
            {
                case "inst":
                    PrintLoadedStageNames();
                    break;

                case "file":
                    PrintLoadedStageMetaFileNames();
                    break;

                default: break;
            }
        }


        private static void AddChild(params object[]? e)
        {
            if (!TryGetNodeByNameAtIndex(e, out var node, 0)) return;
            node.Child(Player.test_child_node(node));
        }
        public static bool TryGetNodeByNameAtIndex(object[]? e, out Node node, int index)
        {
            if(!TryGetArgAtIndex(0,out string nodeName, e))
            { 
                Command.Error("node.Child(string parentName)", CmdError.ArgumentNotFound);
                node = null;
                return false;
            }

            Stage? stage = Runtime.Instance.GetStage();

            if (stage is null)
            {
                Command.Error($"node.Child(string {nodeName})", CmdError.NullReference);
                node = null; 
                return false;
            }

            node = stage.FindNode(nodeName);
            
            if (node is null)
            {
                Command.Error($"node.Child(string {nodeName})", CmdError.NullReference);
                return false;
            }

            return true; 

        }

        private static void RandomizeBackground(params object[]? args)
        {
            var stage = Runtime.Instance.GetStage();

            var background = stage.InitializedBackground;

            if (background == null)
            {
                background = stage.GetBackground();
                if (background == null)
                {
                    Error("Error finding background. Instantiating a new one, though this likely does not fix the problem.", 2);
                    background = new(pixel_renderer.Constants.ScreenW, pixel_renderer.Constants.ScreenH);
                }
            }

            int y, x;

            x = background.Width;
            y = background.Height;

            for (int i = 0; i < x - 1; ++i)
                for (int j = 0; j < y - 1; ++j)
                    background.SetPixel(i, j, JRandom.Color());

            Runtime.Instance.renderHost.MarkDirty();
        }
        private static void PrintNodeInformation(Node node)
        {
            Print(
                $"Node Found! " +
                $"\n Name : {node.Name} " +
                $"\n Position : {node.Position.AsString()} " +
                $"\n UUID : {node.UUID} " +
                $"\n Tag: {node.tag} " +
                $"\n Component Count : {node.ComponentsList.Count}");
        }
        private static async void AssetExists(object[]? obj) {

            var found = AssetLibrary.Fetch<Asset>(out List<Asset> assets);
            if (found)
            {
                var prompt = PromptAsync($"found {assets.Count} assets. Do you want more information?");
                await prompt; 
                switch (prompt.Result)
                {
                    case PromptResult.Yes:
                        PrintAssetsInfo(assets);
                        return;
                    case PromptResult.No:
                        Print("Asset fetch cancelled.");
                        return;
                    case PromptResult.Ok:
                        PrintAssetsInfo(assets);
                        return;
                    case PromptResult.Cancel:
                        Print("Asset fetch cancelled.");
                        return;
                    case PromptResult.Timeout:
                        Print("Asset fetch timed out.");
                        return;
                }
              
            }
            void PrintAssetsInfo(List<Asset> assets)
            {
                string assetInfo = "";
                for (int i = 0; i < assets.Count; i++)
                {
                    Asset? asset = assets[i];
                    assetInfo += $"fetch #{i} :: Name, {asset.Name} UUID, {asset.UUID}\n";
                }
                Error(assetInfo, 1);
            }


        }
        private static void ListNodes(params object[]? e)
        {
            string nodesList = "";
            char nonBreakingspace = '\u2007';
            foreach (Node node in Runtime.Instance.GetStage().nodes)
                nodesList += node.Name + " ";
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
