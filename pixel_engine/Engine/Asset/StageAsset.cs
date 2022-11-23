
namespace pixel_renderer.Assets
{
    public class StageAsset : Asset
    {
        public Stage RuntimeValue;
        public StageAsset(string name , Stage runtimeValue) : base(name, typeof(Stage))
        {
           Name = name;
           RuntimeValue = runtimeValue;
        }
    }
}