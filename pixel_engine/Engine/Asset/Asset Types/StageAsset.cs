
using Newtonsoft.Json;
using pixel_renderer.IO;
using System.Collections.Generic;
using System.Drawing;

namespace pixel_renderer.Assets
{
    public class StageAsset : Asset
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
        public StageAsset(string name, List<NodeAsset> nodes, Metadata background, StageSettings settings, string UUID) : base(name, typeof(Stage), UUID)
        {
            this.nodes = nodes;
            this.m_background = background;
        }
        /// <summary>
        /// User Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="runtimeValue"></param>
        public StageAsset(string name, Stage runtimeValue) : base(name, typeof(Stage))
        {
           nodes = runtimeValue.Nodes.ToNodeAssets();
            // Initialize background metadata
        }
        public List<NodeAsset> nodes;
        
        public Metadata m_background; 
        
        public Stage Copy()
        {
            // Import backgruond and output stage with background bitmap loaded;
            // REPLACE NULL REFERENCE TO BACKGROUND
            var output = new Stage(Name, null, nodes, UUID);
            return output;  
        }
        internal Bitmap? GetBackground() => new Bitmap(m_background.fullPath) ?? null;
        public static StageAsset Default => new("Default Stage", StagingHost.Default());
    }
}