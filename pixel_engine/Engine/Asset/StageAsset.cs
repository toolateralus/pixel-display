
using Newtonsoft.Json;
using System.Collections.Generic;

namespace pixel_renderer.Assets
{
    public class StageAsset : Asset
    {
        public List<NodeAsset> nodes;
        public BitmapAsset background;
        public StageSettings settings = new("","");
        /// <summary>
        /// Copies the settings, background, and nodes from the asset to a usable runtime instance.
        /// </summary>
        /// <returns> A copy of the stage asset as an instance of Stage</returns>
        public Stage Copy() { return new(settings.name, background.RuntimeValue, nodes.ToNodeList() ); }
        public StageAsset(string name, Stage runtimeValue) : base(name, typeof(Stage))
        {
           Name = name;
           nodes = runtimeValue.Nodes.ToNodeAssets();
           background = runtimeValue.Background;
           settings = runtimeValue.Settings; 
        }
        [JsonConstructor]
        public StageAsset(List<NodeAsset> nodes, BitmapAsset background, StageSettings settings)
        {
            this.nodes = nodes;
            this.background = background;
            this.settings = settings;
        }
    }

}