
using Newtonsoft.Json;
using System.Collections.Generic;

namespace pixel_renderer.Assets
{
    public record StageAsset : Asset
    {
        /// <summary>
        /// Do not use this, this is open only for file reading purposes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nodes"></param>
        /// <param name="background"></param>
        /// <param name="settings"></param>
        /// <param name="UUID"></param>
        [JsonConstructor]
        public StageAsset(string name, List<NodeAsset> nodes, BitmapAsset background, StageSettings settings, string UUID) : base(name, typeof(Stage), UUID)
        {
            this.nodes = nodes;
            this.background = background;
            this.settings = settings;
        }
        /// <summary>
        /// User Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="runtimeValue"></param>
        public StageAsset(string name, Stage runtimeValue) : base(name, typeof(Stage))
        {
           nodes = runtimeValue.Nodes.ToNodeAssets();
           background = runtimeValue.Background;
            settings = new(runtimeValue.Name, runtimeValue.UUID);
        }
        public List<NodeAsset> nodes;
        public BitmapAsset background;
        public StageSettings settings;
        public static StageAsset? Default => new StageAsset("Default Stage", StagingHost.Default());
        public Stage Copy()=> new("FROM_ASSET", background, nodes);
    }
}