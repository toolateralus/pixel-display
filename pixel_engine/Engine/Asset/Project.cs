using System.Collections.Generic;
using pixel_renderer;
using pixel_renderer.IO;
using pixel_renderer.Assets;
using System.DirectoryServices.ActiveDirectory;
using Newtonsoft.Json;

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
        public List<Asset> library;
        public int stageIndex;
        public int fileSize = 0;

    /// <summary>
    /// use this for new projects and overwrite the default stage data, this prevents lockups
    /// </summary>
    /// <param name="name"></param>
        public Project(string name)
        {
            Name = name;
            library = Library.Clone(); 
            stageIndex = 0;
            fileSize = 10;
        }
        [JsonConstructor]
        public Project(List<StageAsset> stages, List<Asset>  library, int stageIndex, int fileSize, string name)
        {
            this.stages = stages;
            this.library = library;
            this.stageIndex = stageIndex;
            this.fileSize = fileSize;
            Name = name;
        }

    public string Name { get; }

    }
