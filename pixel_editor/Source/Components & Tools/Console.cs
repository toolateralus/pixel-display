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
        public static CSFunction help() => new((args) => { 
            foreach (var item in CSFunction.GetCSLibrary())
            {
                var info = CSFunction.GetFunctionInfo(item);
                Print($"{item.Value} : {info}");
            }

        }, "Shows a list of all available functions and some info");
        private static void PopulateCommandLists()
        {
            var type = Current.GetType();
            var methods = type.GetRuntimeMethods();

            foreach (var method in methods)
                if (method.ReturnType == typeof(CSFunction))
                {
                    CSFunction? item = (CSFunction)method.Invoke(null, null);
                    string name = method.Name;
                    item.Value = name;
                }

            for (int i = 0; i < LUA.functions_list.Count; i++)
            {
                LuaFunction? item = LUA.functions_list[i];
                CSFunction cmd = new((a) => item.Invoke(LUA.GetHandle()) , $"LUA {i}");
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
