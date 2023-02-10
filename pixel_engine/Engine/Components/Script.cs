using pixel_renderer.FileIO;
using System;

namespace pixel_renderer.Assets
{
    public class ScriptAsset : Asset
    {
        public Component component;
        public ScriptAsset(Component c, string name, Type type) : base(name, c.UUID)
        {
            component = c;
        }

      
    }
}
