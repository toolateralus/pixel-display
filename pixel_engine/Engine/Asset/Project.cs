using System.Collections.Generic;
using pixel_renderer;
using pixel_renderer.IO;
using pixel_renderer.Assets;

public class Project
    {
        public static Project LoadProject()
        {
            Project project = new("Default");
            Dialog dlg = Dialog.ImportFileDialog();

            if (dlg.type is null)
                return project;

            if (dlg.type.Equals(typeof(Project)))
                project = ProjectIO.ReadProjectFile(dlg.fileName);

                if (project is not null)
                    return project;
                    else return new("Default"); 
        }
        public List<StageAsset> stages;
        public Library library;
        public int stageIndex;
        public int fileSize = 0;

        public Project(string name)
        {
            Name = name;
            StageAsset asset = new("", Staging.Default());  
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
