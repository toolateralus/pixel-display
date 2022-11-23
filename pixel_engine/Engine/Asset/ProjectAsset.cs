using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace pixel_renderer.Assets
{
    public class Project 
    {
        public Settings settings; 
        public Runtime runtime;
        public List<StageAsset> stages; 
        public int stageIndex;
        public int fileSize = 0;

        public Project(string name) 
        {
            Name = name;
            settings = new();
            runtime = Runtime.Instance;
            stages = new(); 
            stageIndex = 0;
            fileSize = 10; 
        }

        public string Name { get; }

    }
}