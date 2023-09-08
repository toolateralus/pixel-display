using Pixel.Statics;
using Pixel.Types.Physics;
using PixelLang.Tools;
using System;
using System.Collections.Generic;

namespace Pixel
{
    public static class PLangHook
    {
        public static int[] handles = new int[64];
        public static Dictionary<int , Action> unhook = new(); 
        
        public const int HRENDER = 0;
        public const int HPHYSICS = 1;

        public static int AttachHook(Hook hook, string functionBody)
        {
            switch (hook)
            {
                case Hook.Render:
                    return render_hook(functionBody);
                case Hook.Physics:
                    return physics_hook(functionBody);
                case Hook.Awake:
                    return 0;
                case Hook.Destroy:
                    return 0;
            }
            return -1; 

            int physics_hook(string functionDeclaration)
            {
                var newFunction = GetNextHook(handles[HPHYSICS]++, in functionDeclaration, out var name);

                Language.TryCallLine(newFunction);

                Physics.OnStepComplete += hook_function;

                unhook.Add(handles[HPHYSICS], delegate
                {
                    Runtime.Current.renderHost.OnRenderCompleted -= hook_function;
                });

                return handles[HPHYSICS];

                void hook_function(double _) => Language.TryCallLine($"{name}();");
            }

            int render_hook(string functionDeclaration)
            {
                var newFunction = GetNextHook(handles[HRENDER]++,  in functionDeclaration, out var name);

                Language.TryCallLine(newFunction);

                Runtime.Current.renderHost.OnRenderCompleted += hook_function;

                unhook.Add(handles[HRENDER], delegate
                {
                    Runtime.Current.renderHost.OnRenderCompleted -= hook_function;
                });

                return handles[HRENDER];

                void hook_function(double _) => Language.TryCallLine($"{name}();");
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
            if (unhook.ContainsKey(handle))
                return;

            unhook[handle]?.Invoke();
            unhook.Remove(handle);
        }
        public static string GetNextHook(int handle,  in string function, out string name)
        {
            name = get_render_hook_function_name();

            var decl_str = $"function {name}() {{{function}}};";
            return decl_str;
            string get_render_hook_function_name() => $"Z{UUID.NewUUID()}{handle}";
        }
    }
}
