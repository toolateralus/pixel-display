using Pixel.Types.Components;
using PixelLang.Interpreters;
using PixelLang.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Pixel
{
    public class HookTest : Component
    {

        [Method]
        public void Test()
        {
            PLangHook.HookIntoRenderLoop("");
            InputProcessor.TryCallLine(
                "var num = 0; " +
                "function Update { " +
                "                  " +
                "                   " +
                "                   " +
                "                   " +
                " }; ");
               
               
               
               
               
               
               
        }

        public override void Dispose()
        {

        }
    }

    public static class PLangHook
    {
        public static int[] handles = new int[64];
        public static List<Action> unhook = new(); 
        public const int HRENDER = 0;
        
        static PLangHook()
        {
            handles[HRENDER] = 1;
        }

        public static void HookIntoRenderLoop(string functionDeclaration)
        {
            var funct_decl = GetNextHook(handles[HRENDER]++, functionDeclaration, out var name);

            InputProcessor.TryCallLine(funct_decl);

            Runtime.Current.renderHost.OnRenderCompleted += hook_function;

            unhook.Add(delegate
            {
                Runtime.Current.renderHost.OnRenderCompleted -= hook_function;
            });

            void hook_function(double _) => InputProcessor.TryCallLine($"=> {name} ;");
        }

        

        public static void UnhookAll()
        {
            foreach (var hook in unhook)
                hook?.Invoke();
            unhook.Clear();
        }
        public static string GetNextHook(int handle, string decl, out string name)
        {
            name = get_render_hook_function_name();
            var decl_str = $"function {name} {{{decl }}} ;";
            return decl_str;
            string get_render_hook_function_name() => $"Render{handle}";
        }
    }

    public static class PLangGen
    {
       
    }
}
