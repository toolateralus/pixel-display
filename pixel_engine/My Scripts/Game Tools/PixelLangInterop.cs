using Pixel.Types.Components;
using PixelLang.Tools;
using PixelLang.Types;
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
            node.AddComponent<PixelLangHook>();
            return node;
        }
    }

    public enum Hook { Render, Physics, Awake, Destroy } 
}
