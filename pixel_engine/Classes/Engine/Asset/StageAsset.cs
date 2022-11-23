using System;

namespace pixel_renderer.Assets
{
    public class StageAsset : Asset
    {
        public Stage RuntimeValue;
        public StageAsset(string name, Type? fileType, Stage runtimeValue) : base(name, fileType)
        {
            this.fileType = typeof(Stage);
            this.Name = name;
            this.RuntimeValue = runtimeValue;
        }
    }
}