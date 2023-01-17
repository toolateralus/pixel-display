using Newtonsoft.Json;
using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System.Collections.Generic;

namespace pixel_renderer
{
    public class Project
    {
        public int fileSize = 0;
        public List<Asset> library;
        public int stageIndex;
        public List<StageAsset> stages;
        public string Name 
        { 
            get; 
            private set; 
        }
        private int hash;

        private int NameHash()
        {
            object[] o = new object[] { Name, stages };
            return o.GetHashCode();
        }
        public (string, int) Rename(string newName, int hash)
        {
            if (this.hash.Equals(hash))
                Name = newName;
            return (Name, hash);
        }
        public static void SaveProject()
        {
            Project? proj;
            Metadata meta;
            GetProjectPsuedoMetadata(out proj, out meta);
            ProjectIO.WriteProject(proj, meta);
        }
        private static void GetProjectPsuedoMetadata(out Project? proj, out Metadata meta)
        {
            proj = Runtime.Instance.LoadedProject;

            proj ??= new("FallbackProject");

            var projDir = Constants.ProjectsDir;
            var rootDir = Constants.WorkingRoot;
            var ext = Constants.ProjectFileExtension;
            var path = rootDir + projDir + '\\' + proj.Name + ext;
            meta = new Metadata(proj.Name, path, ext);
        }
        public static Project LoadProject()
        {
            Project project = new("Default");
            Metadata meta = FileDialog.ImportFileDialog();
            Project loadedProject = IO.ReadJson<Project>(meta);
            return loadedProject is null ? project : loadedProject;
        }
        internal static string GetPathFromRoot(string filePath)
        {
            var output = filePath.Replace(Constants.WorkingRoot, "");
            return output; 
        }

        /// <summary>
        /// use this for new projects and overwrite the default stage data, this prevents lockups
        /// </summary>
        /// <param name="name"></param>
        public Project(string name)
        {
            Name = name;
            library = AssetLibrary.Clone();
            stageIndex = 0;
            fileSize = 0;
            hash = NameHash();
        }
        public Project()
        {

        }

        [JsonConstructor]
        public Project(List<StageAsset> stages, List<Asset> library, int stageIndex, int fileSize, string name, int hash)
        {
            this.stages = stages;
            this.library = library;
            this.stageIndex = stageIndex;
            this.fileSize = fileSize;
            this.hash = hash;
            Name = name;
        }
    }
}