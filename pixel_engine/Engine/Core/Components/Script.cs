using System;

namespace pixel_renderer.Assets
{
    public class ScriptAsset : Asset
    {
        public Component component;
        public ScriptAsset(Component c, string name, Type type) : base(name, type)
        {
            component = c;
        }
    }
}
