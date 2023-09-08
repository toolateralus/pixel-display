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
                //PopulateCommandLists();
            }
        }
      
        public static ExternFunction reimport() => new((i,args) => { Importer.Import(); return Token.null_token; }, "runs the importer and refreshes the asset library.");
        public static ExternFunction getnode() => new((i,args) => {

            Stage? stage = Runtime.Current.GetStage();
            var node = stage.FindNode(args.First().StrVal);
            InspectorControl.SelectNode(node);
            if (node is null)
                return new(false);
            return new(true);

        }, "runs the importer and refreshes the asset library.");
        public static ExternFunction listnodes() => new((intrptr ,args) => {

            var stage = Runtime.Current.GetStage();

            int i = 0;
            foreach (var item in stage.nodes)
                Print(i++ + " : " + item.Name);

            return Token.null_token;

        }, "runs the importer and refreshes the asset library.");
        private static void PopulateCommandLists()
        {
            var type = Current.GetType();
            var methods = type.GetRuntimeMethods();

            foreach (var method in methods)
                if (method.ReturnType == typeof(ExternFunction))
                {
                    ExternFunction? item = (ExternFunction)method.Invoke(null, null);
                    string name = method.Name;
                    item.StrVal = name;
                }

            for (int i = 0; i < LUA.functions_list.Count; i++)
            {
                LuaFunction? item = LUA.functions_list[i];
                ExternFunction cmd = new((i,a) =>
                {
                    IntPtr hdnl = LUA.GetHandle();
                    int value = item.Invoke(hdnl);
                    return new(value);
                }, $"LUA {i}");
                string name = ParseMethodName(item);
                cmd.StrVal = name;
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
            EditorEvent editorEvent = new EditorEvent(EditorEventFlags.CLEAR_CONSOLE);
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
