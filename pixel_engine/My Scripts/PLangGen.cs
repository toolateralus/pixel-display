﻿using Pixel.Statics;
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
        int? hndl;
        [Method]
        public void Test()
        {
            InputProcessor.TryCallLine("var num = 0; \n function myFunctionName { \n => clear; \n num = 2 * num; \n => print num; \n }; ");
            hndl = PLangHook.AttachHook(Hook.Render, " => myFunctionName; ");
        }
        [Method]
        public void Unhook()
        {
            if(hndl.HasValue)
                PLangHook.Unhook(hndl.Value);
        }

        public override void Dispose()
        {

        }
    }

    public enum Hook { Render, Physics, Awake, Destroy } 
    public static class PLangHook
    {
        public static int[] handles = new int[64];
        public static Dictionary<int , Action> unhook = new(); 
        public const int HRENDER = 0;
        public static int AttachHook(Hook hook, string functionDeclaration)
        {
            switch (hook)
            {
                case Hook.Render:
                    return render_hook(functionDeclaration);
                case Hook.Physics:
                    return 0;
                case Hook.Awake:
                    return 0;
                case Hook.Destroy:
                    return 0;
            }
            return -1; 

            int render_hook(string functionDeclaration)
            {
                var newFunction = GetNextHook(handles[HRENDER]++, in functionDeclaration, out var name);

                InputProcessor.TryCallLine(newFunction);

                Runtime.Current.renderHost.OnRenderCompleted += hook_function;

                unhook.Add(handles[HRENDER], delegate
                {
                    Runtime.Current.renderHost.OnRenderCompleted -= hook_function;
                });

                return handles[HRENDER];

                void hook_function(double _) => InputProcessor.TryCallLine($"=> {name} ;");
            }
        }
        public static void UnhookAll()
        {
            foreach (var hook in unhook)
                hook.Value?.Invoke();
            unhook.Clear();
        }
        public static void Unhook(int handle)
        {
            unhook[handle]?.Invoke();
            unhook.Remove(handle);
        }
        public static string GetNextHook(int handle, in string function, out string name)
        {
            name = get_render_hook_function_name();
            var decl_str = $"function {name} {{{function }}} ;";
            return decl_str;
            string get_render_hook_function_name() => $"RuntimeFunction{handle}";
        }
    }
}
