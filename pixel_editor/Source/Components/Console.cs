using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
namespace pixel_editor
{
    public class Console
    {
        private const string divider = "-- -- --";
        public static Console Current
        {
            get
            {
                if (current == null)
                {

                    // init singleton and use it to fill cmd list; 
                    current = new();
                    var type = current.GetType();
                    var methods = type.GetRuntimeMethods();
                    foreach (var method in methods)
                        if (method.ReturnType == typeof(Command) && method.Name.Contains("cmd_"))
                        {
                            Command? item = (Command)method.Invoke(null, null);
                            if (item != null && !Current.LoadedCommands.Contains(item))
                                Current.LoadedCommands.Add(item);
                        }
                }

                return current;
            }
            set => current = value;
        }
        private static Console current;
        #region General Commands
        public static Command cmd_help() => new()
        {
            phrase = "help;",
            syntax = "help();",
            action = (o) =>
            {
                string output = "";
                foreach (var cmd in Current.LoadedCommands)
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
        public static Command cmd_echo() => new()
        {
            phrase = "echo;",
            syntax = "echo(message);",
            argumentTypes = new string[] { "str:" },
            action = (o) => Console.Print(o[0]),
            description = "Logs a message to the console, Some characters will cause this to fail."
        };
        #endregion
        #region Renderer Commands
        public static Command cmd_set_resolution() => new()
        {
            phrase = "resolution.Set;",
            syntax = "resolution.Set(x,y);",
            argumentTypes = new string[] { "vec:" },
            action = (e) =>
            {
                if (TryGetArgAtIndex(0, out Vector2 vec, e))
                    return;

                Print(vec.ToString());
                Runtime.Current.projectSettings.CurrentResolution = vec;
                Runtime.Current.SetResolution();

            },
            description = "sets the resolution to the specified Vec2"
        };
        public static Command cmd_get_resolution() => new()
        {
            phrase = "resolution.Get;",
            syntax = "resolution.Get();",
            action = (e) =>
            {
                Console.Print(Runtime.Current.renderHost.GetRenderer().Resolution.ToString());
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
        public static Command cmd_set_sprite_filtering() => new()
        {
            phrase = "sprite.Filtering;",
            syntax = "sprite.Filtering(string: nodeName, string: enumValue);",
            argumentTypes = new string[] { "str:", "str:" },
            args = null,
            action = SetSpriteFiltering,
            description = "Gets a node and attempts to set the texture filtering on the sprite if available.",

        };
        private static void SetSpriteFiltering(object[]? obj)
        {
            if (obj is null || obj.Length != 2)
                Command.Error("SetSpriteFiltering", CmdError.ArgumentNotFound);
            if (!TryGetNodeByNameAtIndex(obj, out Node node, 0))
            {
                Command.Error("SetSpriteFiltering", $"Node \"{obj?[0]}\" not found in hierarchy.");
                return;
            }
            if (node.GetComponent<Sprite>() is not Sprite sprite)
            {
                Command.Error("SetSpriteFiltering", $"Sprite not found on node \"{node.Name}\".");
                return;
            }
            if (!TryGetArgAtIndex(1, out string filteringString, obj))
            {
                Command.Error("SetSpriteFiltering", CmdError.InvalidCast);
                return;
            }
            if (Enum.Parse(typeof(TextureFiltering), filteringString) is not TextureFiltering textureFiltering)
            {
                Command.Error("SetSpriteFiltering", $"\"{filteringString}\" not valid TextureFiltering enum.");
                return;
            }
            sprite.textureFiltering = textureFiltering;
        }

        public Dictionary<string, string> extended_help = new()
        {
            {"help", "type help{cmd name} name to get a more in depth description of the function/usages of a command"},


        };
        public static Command cmd_help_cmd() => new()
        {
            phrase = "help;",
            syntax = "help {cmd name}",
            argumentTypes = new string[] { "str:" },
            action = (e) =>
            {



            },
            args = null,
            description = "gets more help for a specified command"

        };
        public static Command cmd_get_node() => new()
        {
            phrase = "node.Get;",
            syntax = "node.Get(nodeName);",
            argumentTypes = new string[] { "str:" },
            action = (e) =>
            {
                string name = (string)e[0];
                Node node = Runtime.Current.GetStage().FindNode(name);
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
            action = (o) => Runtime.Current.GetStage().AddNode(Rigidbody.Standard()),
            description = "Spawns a generic node with a Rigidbody , Sprite, and Collider, and adds it to the current Stage."
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
                if (!TryGetArgAtIndex(1, out Vector2 vec, e)) return;
                node.Move(vec);
            },
        };
        public static Command cmd_destroy_node() => new()
        {
            phrase = "node.Destroy;",
            syntax = "node.Destroy(null || string nodeName);",
            description = "Attempts to destroy the selected node or node by name. This command can either take no arguments or a string for a node name.",
            argumentTypes = new string[] { "str:" },
            action = (e) =>
            {
                if (!TryGetNodeByNameAtIndex(e, out Node node, 0))
                {
                    if (Editor.Current.LastSelected != null || Editor.Current.ActivelySelected.Count > 0)
                    {
                        Editor.Current.LastSelected?.Destroy();

                        foreach (var selected in Editor.Current.ActivelySelected)
                            selected?.Destroy();
                    }
                    return;
                }
                node?.Destroy();
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

                Node? node = Runtime.Current.GetStage().FindNode(nName);
                if (node is null)
                    Console.Print("Node was not found.");
                Camera cam = node.GetComponent<Camera>();
                Type type = cam.GetType();
                FieldInfo? field = type.GetRuntimeField(fName);
                field.SetValue(cam, value);
            },
            description = "\n {must not be a property or method} Sets Field in camera by name on node of provided name \n syntax : cam(str:<nodeName>, str:<fieldName>, object:value)",
        };
        public static Command cmd_reimport() => new()
        {
            description = "Reimports and refreshes asset library, does not force update references.",
            phrase = "import;",
            syntax = "import();",
            argumentTypes = null,
            action = (e) =>
            {
                Editor.Current.fileViewer?.Refresh();
                Importer.Import();
            },
        };
        public static Command cmd_asset_exists() => new()
        {
            description = "Shows a count of all loaded assets, and an option to see more info.",
            phrase = "fetch;",
            syntax = "fetch(assetName);",
            argumentTypes = new string[] { "str:" },
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
                            Runtime.Current.SetProject(project);
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
        public static Command cmd_load_project_2() => new()
        {
            phrase = "load;",
            syntax = "load();",
            argumentTypes = null,
            action = (e) =>
            {
                string name = (string)e[0];
                var project = Project.Load();
                
                if (project != null)
                    Runtime.Current.SetProject(project);

            },
            args = new object[] { },
            description = "Runs a Load-Project dialog."
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

                Project project = Runtime.Current.project;

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
                        Runtime.Current.SetStage(stage);
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
                Project.TryLoadStage(index);
            },

        };
        public static Command cmd_new_stage() => new()
        {
            phrase = "stage = new;",
            syntax = "stage = new();",
            description = "opens the stage wizard",
            argumentTypes = null,
            action = (o) =>
            {
                Runtime.Log("stageWnd opened.");
                Editor.Current.stageWnd = new(Editor.Current);
                Editor.Current.stageWnd.Show();
            },
            args = null,
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
        #region Editor Commands
        public static Command cmd_set_stage_background_relative()
        {
            var x = new Command(); 
            x.phrase = "stage.SetBackground;";
            x.syntax = "stage.SetBackground(string assetNameRelativeToWorkingRoot);";
            x.action = setBackgroundRelative;
            x.argumentTypes = new string[] { "str:" };
            x.description = "Attempts to find asset of provided name and if it's a valid texture, prompts the user to load it as the stages background.";
                return x;
        }
        private static async void setBackgroundRelative(object[]? obj)  
        {
            string name;
            if (!TryGetArgAtIndex(0, out name, obj))
            {
                Command.Error("stage.SetBackground();", 0);
                return;
            }
            var foundMetadata = AssetLibrary.FetchMetaRelative(name);
            if (foundMetadata != null && foundMetadata.extension == pixel_renderer.Constants.PngExt)
            {
                var task = PromptAsync($"Asset {foundMetadata.Name} Found. Do you want to load this background?");
                await task;
                switch (task.Result)
                {
                    case PromptResult.Yes:
                        if (Runtime.Current.GetStage() is not Stage stage)
                        {
                            Command.Error("stage.SetBackground();", "No stage was loaded");
                            return; 
                        }
                        Runtime.Current.GetStage().backgroundMetadata = foundMetadata;
                        var bmp = new Bitmap(foundMetadata.Path);
                        Runtime.Current.GetStage()?.SetBackground(bmp);
                        Runtime.Current.renderHost.GetRenderer().baseImageDirty = true; 
                        Runtime.Log("Background set.");
                        break;
                    case PromptResult.No:
                        Runtime.Log("SetBackground cancelled.");
                        break;
                    case PromptResult.Cancel:
                        Runtime.Log("SetBackground cancelled.");
                        break;
                    case PromptResult.Timeout:
                        Runtime.Log("SetBackground cancelled.");
                        break;
                    default:
                        break;
                }
            }
        }
        public static Command cmd_show_colliders() => new()
        {
            phrase = "showColliders;",
            syntax = "showColliders();",
            action = showColliders,
            description = "highlights all of the colliders bounds in green",
        };
        static bool collidersHighlighted = false;
        private static void showColliders(object[]? obj)
        {
            if (!collidersHighlighted)
            {
                Stage? stage = Runtime.Current.GetStage();

                if (stage is null)
                {
                    Command.Error("showColliders", CmdError.NullReference);
                    return;
                }
                var colliders = stage.GetAllComponents<Collider>();
                foreach (var x in colliders)
                {
                    x.drawCollider = true;
                    x.drawNormals = true;
                }
                collidersHighlighted = true;
            }
            else
            {
                Stage? stage = Runtime.Current.GetStage();

                if (stage is null)
                {
                    Command.Error("showColliders", CmdError.NullReference);
                    return;
                }
                var colliders = stage.GetAllComponents<Collider>();

                foreach (var x in colliders)
                {
                    x.drawCollider = false;
                    x.drawNormals = false;
                }
            }
        }
        public static Command cmd_move_inspector() => new()
        {
            phrase = "inspector.Move;",
            syntax = "inspector.Move(Vec2 newPosition)",
            action = moveInspector,
            description = "Sets the inspectors current position",
            argumentTypes = new string[] { "vec:" },
        };
        private static void moveInspector(object[]? obj)
        {
            if (!TryGetArgAtIndex(0, out Vector2 vec, obj))
            {
                Command.Error("inspector.Position(Vec2 newPosition)", CmdError.ArgumentNotFound);
                return;
            }
            Inspector.OnInspectorMoved.Invoke((int)vec.X, (int)vec.Y);
        }
        #endregion
        public static void Print(object? o, bool includeDateTime = false)
        {
            var msg = o.ToString();
            var e = new EditorEvent(EditorEventFlags.PRINT, msg, includeDateTime);
            Editor.QueueEvent(e);
        }
        public static void Error(object? o = null, int? textColorAlterationDuration = null)
        {
            string? msg = o.ToString();
            EditorEvent e = new(EditorEventFlags.PRINT_ERR, msg, false, false);

            if (textColorAlterationDuration is not null)
                e.action = RedTextAsync((int)textColorAlterationDuration);
            Editor.QueueEvent(e);
        }
        public static async Task<PromptResult> PromptAsync(string question, float? waitDuration = 60f)
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
        public static void Clear(bool randomPixel = false)
        {
            EditorEvent editorEvent = new EditorEvent(EditorEventFlags.CLEAR_CONSOLE, "");
            editorEvent.ClearConsole = true;
            Editor.QueueEvent(editorEvent);

            if (randomPixel)
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
        public static bool TryGetArgAtIndex<T>(int index, out T? arg, object[] o)
        {
            bool hasArg = o != null && o.Length > index;

            if (hasArg && o[index] is T val)
            {
                arg = val;
                return true;
            }
            arg = default;
            return false;
        }
        static void PrintLoadedStageNames()
        {
            foreach (var stage in Runtime.Current.project.stages)
                Console.Print(stage.Name);
        }
        static void PrintLoadedStageMetaFileNames()
        {
            foreach (var stage in Runtime.Current.project.stagesMeta)
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
        public static bool TryGetNodeByNameAtIndex(object[]? e, out Node? node, int index)
        {
            if (!TryGetArgAtIndex(0, out string nodeName, e))
            {
                Command.Error("GetNodeAtIndex", CmdError.ArgumentNotFound);
                node = null;
                return false;
            }
            Stage? stage = Runtime.Current.GetStage();
            if (stage is null)
            {
                Command.Error("GetNodeAtIndex", CmdError.NullReference);
                node = null;
                return false;
            }
            node = stage.FindNode(nodeName);
            if (node is null)
            {
                Command.Error("GetNodeAtIndex", CmdError.NullReference);
                return false;
            }
            return true;
        }
        private static void PrintNodeInformation(Node node)
        {
            Print(
                $"Node Found! " +
                $"\n Name : {node.Name} " +
                $"\n Position : {node.Position} " +
                $"\n UUID : {node.UUID} " +
                $"\n Tag: {node.tag} " +
                $"\n Component Count : {node.Components.Count}");
        }
        private static async void AssetExists(object[]? obj)
        {

            if(!TryGetArgAtIndex<string>(0, out var arg, obj )) return;
            var assets = AssetLibrary.FetchMeta(arg);
            if (assets is null)
            {
                Print($"File {arg} was not found.");
                return; 
            }
            var prompt = PromptAsync($"File at {arg} found. Do you want more information?");
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
            void PrintAssetsInfo(Metadata meta)
            {
                string assetInfo = $"Name, {meta?.Name} \n Path, {meta?.Path} Ext {meta?.extension}\n";
                Error(assetInfo, 1);
            }
        }
        private static void ListNodes(params object[]? e)
        {
            string nodesList = "";
            foreach (Node node in Runtime.Current.GetStage().nodes)
                nodesList += node.Name + " ";
            Print($"{nodesList}");
        }
        private static void SetNodeField(params object[]? e)
        {
            if (e.Length >= 3)
            {
                string nName = (string)e[0];
                string fName = (string)e[1];
                object value = e[2];

                Node? node = Runtime.Current.GetStage().FindNode(nName);
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

                if (Runtime.Current.GetStage().FindNode(nName) is not Node node)
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
        public List<Command> LoadedCommands = new();
    }
}
