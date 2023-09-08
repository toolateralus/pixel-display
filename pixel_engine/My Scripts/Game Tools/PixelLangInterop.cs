using LibNoise.Modifier;
using Pixel.Types.Components;
using PixelLang.Tools;
using PixelLang.Types;
using System;
using System.Collections.Generic;

namespace Pixel
{

    public class PixelLangHook : Component
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
            Language.TryCallLine(code);

            ExternFunction.InjectFunction(funct: new ExternFunction(TestFunction, "") { StrVal = "get_frametime" });

            hndl = PLangHook.AttachHook(Hook.Render, $"get_frametime");
        }

        private Token TestFunction(PixelLang.Interpreters.Interpreter arg1, List<Token> arg2)
        {
            throw new NotImplementedException();
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
            node.AddComponent<PixelLangHook>();
            return node;
        }
    }

    public enum Hook { Render, Physics, Awake, Destroy } 
}
