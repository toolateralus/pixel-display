using Newtonsoft.Json.Serialization;
using Pixel.Statics;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using PixelLang.Interpreters;
using PixelLang.Tools;
using PixelLang.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Pixel
{
 
 

    public class HookTest : Component
    {
        int? hndl;

        [Field]
        string code = @"    
            
            i = 100;
            x = 25;

            function main(int arg) {
                return 0;
            };


        ";
        [Method]
        public void HookIntoRenderLoop()
        {
            Token cached = Token.null_token;
            InputProcessor.TryCallLine(code);

            ExternFunction.InjectFunction(new(F, "") { StrVal = "get_frametime" });

            Token F(List<Token> args)
            {
                cached.NumVal = (Runtime.Current.renderHost.info.frameCount);
                return cached;
            }

            hndl = PLangHook.AttachHook(Hook.Render, $"get_frametime");
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

        public static Node Standard()
        {
            var node = Rigidbody.Standard();
            node.AddComponent<HookTest>();
            return node;
        }
    }

    public enum Hook { Render, Physics, Awake, Destroy } 
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

                InputProcessor.TryCallLine(newFunction);

                Physics.OnStepComplete += hook_function;

                unhook.Add(handles[HPHYSICS], delegate
                {
                    Runtime.Current.renderHost.OnRenderCompleted -= hook_function;
                });

                return handles[HPHYSICS];

                void hook_function(double _) => InputProcessor.TryCallLine($"{name}();");
            }

            int render_hook(string functionDeclaration)
            {
                var newFunction = GetNextHook(handles[HRENDER]++,  in functionDeclaration, out var name);

                InputProcessor.TryCallLine(newFunction);

                Runtime.Current.renderHost.OnRenderCompleted += hook_function;

                unhook.Add(handles[HRENDER], delegate
                {
                    Runtime.Current.renderHost.OnRenderCompleted -= hook_function;
                });

                return handles[HRENDER];

                void hook_function(double _) => InputProcessor.TryCallLine($"{name}();");
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
