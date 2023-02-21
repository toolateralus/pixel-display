using pixel_renderer.FileIO;
using System;

namespace pixel_renderer.Assets
{
    public class ScriptAsset : Asset
    {
        public Component component;
        public ScriptAsset(Component runtimeValue, string name = "Script Asset") : base(name, false)
        {
            component = runtimeValue;
        }

      
    }
}
