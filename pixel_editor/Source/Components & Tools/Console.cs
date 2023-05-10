using KeraLua;
using Pixel;
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Statics;
using Pixel.Types;
using PixelLang.Tools;
using PixelLang.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
namespace Pixel_Editor
{
    public class Console
    {
        private static Console current;
        public static Console Current
        {
            get => current;
            set => current = value;
        }
        static Console()
        {
            if (current == null)
            {
                current = new();
                PopulateCommandLists();
            }
        }
        public static Function help() => new((args) => { 
            foreach (var item in Function.Library())
            {
                var info = Function.GetInfo(item);
                Print($"{item.Value} : {info}");
            }
        }, "Shows a list of all available functions and some info");

        static Function fromFile = new Function(FromFileAsync, "loads and runs code from a .pl file.") { Value = "fromFile" };
        static Function getFiles = new Function(GetFiles, "loads and runs code from a .pl file.") { Value = "getFiles" };
        public static Function cd() => new((args) =>
        {
            Token? token = args.FirstOrDefault();

            if (token is not null && token.Type == TType.STRING)
            {
                var path = token.Value.Replace("\"", "");
                path = path.Replace("\\", "");
                path = path.Replace("\'", "");
                path = path.Replace("/", "");
                path = path.Replace("//", "");

                PLang.SetCutsomScriptFolderPath(path);
                PixelLang.Tools.Console.Log($"Script Path : {PLang.SCRIPT_PATH}");
            }

        }, "Changes the directory which scripts are read from, always underneath \\Pixel\\Assets\\  and cannot be in a sub-directory.")
        { Value = "cd"};
        public static async void FromFileAsync(List<Token> args)
        {
            if (args.FirstOrDefault() is Token token && token.Type == TType.STRING && token.Value.Contains(".pl"))
            {
                token.Value = token.Value.Replace("\\", "");
                token.Value = token.Value.Replace("\"", "");

                var path = PLang.GetRootDirectory() + token.Value;
                PixelLang.Tools.Console.Log("reading from path : " + path + "...");
                if (!File.Exists(path))
                {
                    PixelLang.Tools.Console.Log("File not found.");
                    return;
                }
                using var textReader = new StreamReader(path);
                var code = textReader.ReadToEnd();
                textReader.Close();
                PixelLang.Tools.Console.Log("executing from path : " + path + "...");
                await PLang.TryCallLine(code);
            }
        }
        private static void GetFiles(List<Token> obj)
        {
            var files = PLang.GetFiles();

            foreach (var file in files)
                PixelLang.Tools.Console.Log(file);
        }
        public static Function reimport() => new((args) => { Importer.Import(); }, "runs the importer and refreshes the asset library.");
        private static void PopulateCommandLists()
        {
            var type = Current.GetType();
            var methods = type.GetRuntimeMethods();

            foreach (var method in methods)
                if (method.ReturnType == typeof(Function))
                {
                    Function? item = (Function)method.Invoke(null, null);
                    string name = method.Name;
                    item.Value = name;
                }

            for (int i = 0; i < LUA.functions_list.Count; i++)
            {
                LuaFunction? item = LUA.functions_list[i];
                Function cmd = new((a) => item.Invoke(LUA.GetHandle()) , $"LUA {i}");
                string name = ParseMethodName(item);
                cmd.Value = name;
            }
        }
        public static void Print(object? o, bool includeDateTime = false)
        {
            var msg = o?.ToString();
            var e = new EditorEvent(EditorEventFlags.PRINT, msg ?? "NULL", includeDateTime);
            EditorEventHandler.QueueEvent(e);
        }
        public static void Error(object? o = null, int? textColorAlterationDuration = null)
        {
            string? msg = o.ToString();
            EditorEvent e = new(EditorEventFlags.PRINT_ERR, msg, false);
            EditorEventHandler.QueueEvent(e);
        }
        public static void Clear(bool randomPixel = false)
        {
            EditorEvent editorEvent = new EditorEvent(EditorEventFlags.CLEAR_CONSOLE, "");
            EditorEventHandler.QueueEvent(editorEvent);

            if (randomPixel)
                Error("Console Cleared", 1);
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
        private static string ParseMethodName(LuaFunction item)
        {
            const string nums = "1234567890<>_";
            var name = item.Method.Name;

            foreach (char disallowed in nums)
                for (int found = 0; found < name.Length; found++)
                {
                    char y = name[found];
                    if (y == disallowed)
                    {
                        found = 0;
                        name = name.Replace($"{y}", "");
                    }
                }
            name = name.Remove(name.Length - 1);
            name += ';';
            return name;
        }
    }
}
