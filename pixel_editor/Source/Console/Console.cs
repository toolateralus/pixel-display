﻿using pixel_renderer;
using pixel_editor;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;

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
        public static void Clear(bool randomColor = false)
        {
            EditorEvent editorEvent = new EditorEvent("");
            editorEvent.ClearConsole = true;
            Editor.QueueEvent(editorEvent);
            
            if(randomColor)
                Error("Console Cleared", 1);
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

        public static Command cmd_swap_theme()
        {
            return new()
            {
                phrase = "theme;",
                description = "Swaps the current theme [DOES NOT WORK CURRENTLY]",
                syntax = "theme(light_or_dark);",
                argumentTypes = new string[] {"str:"},
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
        }
        public static Command cmd_load_project()
        {
            return new()
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
        }
        public static Command cmd_reload_stage()
        {
            return new()
            {
                phrase = "reload;|/r;|++r;",
                syntax = "reload();",
                action = (o) =>
                {
                    Runtime.TryLoadStageFromProject();
                },
                args = null,
                description = "Reloads the currently loaded stage",
            };
        }
        public static Command cmd_random_background()
        {
            return new()
            {
                phrase = "stage.Background.Randomize;",
                syntax = "stage.Background.Randomize();",
                action = RandomizeBackground,
                description = "Sets the current stage's background to a random array of colors until reloaded.",
            };
        }
        public static Command cmd_set_stage()
        {
            return new()
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
        }
        public static Command cmd_asset_exists()
        {
            return new()
            {
                description = "Shows a count of all loaded assets, and an option to see more info.",
                syntax = "fetch(assetName);",
                argumentTypes = new string[] { "str:" },
                phrase = "fetch;",
                action = AssetExists,
            };
        }
        public static Command cmd_get_node()
        {
            return new()
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
        }
        public static Command cmd_list_node()
        {
            return new()
            {
                phrase = "node.List;",
                syntax = "node.List();",
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
                syntax = "node.Set(nodeName, fieldName, value);",
                argumentTypes = new string[] { "str:", "str:", "bool:" },
                args = null,
                action = SetNodeField,
                description = "gets a node and attempts to write the provided value to specified field.",
            
            };
        }
        public static Command cmd_call_node_method()
        {
            return new()
            {
                phrase = "node.Call;",
                syntax = "node.Call(nodeName, methodName);",
                argumentTypes = new string[] {"str:", "str:"},
                action = CallNodeMethod,
                args = null,
                description = "Gets a node by Name, finds the provided method by MethodName, and invokes the method.",
                             
            };
        }
        public static Command cmd_set_resolution()
        {
            return new()
            {
                phrase = "resolution.Set;",
                syntax = "resolution.Set(1024,1024);",
                argumentTypes = new string[] { "vec:"},
                action = (e) =>
                {
                    Vec2 vector = (Vec2)e[0];
                    Vec2Int newRes = (Vec2Int)vector;
                    Console.Print(vector.AsString());
                    Runtime.Instance.renderHost.GetRenderer().Resolution = newRes;
                },
                description = "sets the resolution to the specified Vec2"
            };

        }
        public static Command cmd_get_resolution()
        {
            return new()
            {
                phrase = "resolution.Get;",
                syntax = "resolution.Get();",
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
                syntax = "cclear();",
                action = (e) =>
                {
                    if (e is not null && e[0] is bool color)
                        Clear(color);
                    else Clear(); 
                },
                description = "Clears the console's output",
            };
        }

        public static Command cmd_spawn_generic()
        {
            return new()
            {
                phrase = "++n;",
                syntax = "++n();",
                action = (o) => Runtime.Instance.GetStage().create_generic_node(),
                description = "Spawns a generic node with a Rigidbody , Sprite, and Collider, and adds it to the current Stage."
            };

        }
        public static Command cmd_help()
        {
            return new()
            {
                phrase = "help;|help|Help|/h",
                syntax = "help();",
                action = (o) =>
                {
                    string output = "";
                    foreach (var cmd in Current.Active)
                        output += "\n" + cmd.syntax  + "\n" + cmd.description + "\n" + divider;
                    Print(output);
                },
            };
        }
        public static Command cmd_log()
        {
            return new()
            {
                phrase = "log;",
                syntax = "log(message);",
                argumentTypes = new string[] {"str:"},
                action = (o) => Console.Print(o[0]),
                description = "Logs a message to the console, Some characters will cause this to fail."
            };
        }
        public static Command cmd_set_camera()
        {
            return new()
            {
                phrase = "cam;",
                syntax = "cam(Name, Field, Vector2 Value);",
                argumentTypes = new string[] { "str:", "str:", "vec:"},
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
        }

        private static void RandomizeBackground(params object[]? args)
        {
            var stage = Runtime.Instance.GetStage();

            var background = stage.initializedBackground;

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
                $"\n Position : {node.position.AsString()} " +
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
