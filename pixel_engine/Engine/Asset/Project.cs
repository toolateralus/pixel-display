using Newtonsoft.Json;
using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project
    {
        public List<Metadata> library;
        public int fileSize = 0;
        
        public List<StageAsset> stages;
        [JsonProperty]
        public List<Metadata> stagesMeta = new();
        [JsonProperty]  public string Name 
        { 
            get; 
            private set; 
        }
        [JsonProperty]  private readonly int Hash;

        private int NameHash()
        {
            object[] o = new object[] { Name, stages };
            return o.GetHashCode();
        }

        public (string, int) Rename(string newName, int hash)
        {
            if (this.Hash.Equals(hash))
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

            if (string.IsNullOrWhiteSpace(meta.fullPath) || 
                !Path.IsPathFullyQualified(meta.fullPath))
                return project;

            Project? loadedProject = IO.ReadJson<Project>(meta);
            return loadedProject is null ? project : loadedProject;
        }
        internal static string GetPathFromRoot(string filePath)
        {
            var output = filePath.Replace(Constants.WorkingRoot, "");
            return output; 
        }

        public static StageAsset? GetStageByName(string stageName)
        {
            bool gotAsset = AssetLibrary.Fetch(stageName, out StageAsset stage);
            
            if (gotAsset) return stage; 
            return null; 
        }

        /// <summary>
        /// use this for new projects and overwrite the default stage data, this prevents lockups
        /// </summary>
        /// <param name="name"></param>
        public Project(string name)
        {
            Name = name;
            library = AssetLibrary.LibraryMetadata();
            fileSize = 0;
            Hash = NameHash();
        }
        public Project()
        {

        }
        [JsonConstructor]
        public Project(List<Metadata>? stage_meta, List<Metadata> library, int stageIndex, int fileSize, string name, int hash)
        {
            //this.stage_metadata = stage_meta;
            this.library = library;
            this.fileSize = fileSize;
            this.Hash = hash;
            Name = name;
        }
    }
}