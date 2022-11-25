using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using pixel_renderer.Projects; 
namespace pixel_renderer.Assets
{
    public class Project 
    {
        public static Project LoadProject()
        {
            Project project = new("Default"); 
            Dialog dlg = Dialog.ImportFileDialog();

            if (dlg.type is null)
                return project;

            if (dlg.type.Equals(typeof(Project)))
            {
                project = ProjectIO.ReadProjectFile(dlg.fileName); 
                if (project is not null)
                    return project; 
            }
            return project; 
        }
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
            var stage = Staging.Default(); 
            StageAsset asset = new("",  stage);
            stageIndex = 0;
            stages = new()
            {
                asset
            };

            stageIndex = 0;
            fileSize = 10; 
        }

        public string Name { get; }

    }
}