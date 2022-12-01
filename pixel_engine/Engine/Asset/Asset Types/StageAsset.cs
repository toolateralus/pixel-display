﻿
using Newtonsoft.Json;
using System.Collections.Generic;

namespace pixel_renderer.Assets
{
    public class StageAsset : Asset
    {
        public List<NodeAsset> nodes;
        public BitmapAsset background;
        public StageSettings settings = new("","");
        public static StageAsset? Default => new StageAsset("Default Stage", StagingHost.Default());
        /// <summary>
        /// Copies the settings, background, and nodes from the asset to a usable runtime instance.
        /// </summary>
        /// <returns> A copy of the stage asset as an instance of Stage</returns>
        /// 
        public Stage Copy()=> new("_IGNORE", background, nodes);
        public StageAsset(string name, Stage runtimeValue) : base(name, typeof(Stage))
        {
           nodes = runtimeValue.Nodes.ToNodeAssets();
           background = runtimeValue.Background;
            settings = new(runtimeValue.Name, runtimeValue.UUID);
        }
        [JsonConstructor]
        public StageAsset(string name, List<NodeAsset> nodes, BitmapAsset background, StageSettings settings, string UUID) : base(name, typeof(Stage), UUID)
        {
            this.nodes = nodes;
            this.background = background;
            this.settings = settings;
        }
    }

}