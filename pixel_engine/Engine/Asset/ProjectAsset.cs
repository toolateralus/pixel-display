using System.Collections.Generic;

namespace pixel_renderer.Assets
{
    public class ProjectAsset :  Asset
    {
        public Settings settings; 
        public Runtime runtime;
        public List<StageAsset> stages; 
        public int stageIndex;

        public ProjectAsset(string name) : base(name, typeof(ProjectAsset))
        {
            Name = name;
            fileSize = "";
            settings = new();
            runtime = Runtime.Instance;
            stages = new(); 
            stageIndex = 0; 
        }
    }
}