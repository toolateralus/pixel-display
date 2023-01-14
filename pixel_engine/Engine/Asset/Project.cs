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
        public List<Metadata> library;
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
        public static Project LoadProject()
        {
            Project project = new("Default");
            Metadata meta = FileDialog.ImportFileDialog();
            Project loadedProject = IO.ReadJson<Project>(meta);
            return loadedProject is null ? project : loadedProject;
        }
        internal static string GetPathFromRoot(string filePath) => filePath.Replace(Constants.WorkingRoot + "\\Pixel", "");
        /// <summary>
        /// use this for new projects and overwrite the default stage data, this prevents lockups
        /// </summary>
        /// <param name="name"></param>
        public Project(string name)
        {
            Name = name;
            GetAssetLibrary();

            stageIndex = 0;
            fileSize = 0;
            hash = NameHash();
        }

        private void GetAssetLibrary()
        {
            library = new();
            var lib = AssetLibrary.Clone();
            foreach (var asset in lib)
                library.Add(asset.Key);
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