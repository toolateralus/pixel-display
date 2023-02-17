using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.IO;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project
    {
        public List<Stage> stages = new();

        [JsonProperty] [Field]
        public List<Metadata> stagesMeta = new();

        [JsonProperty] 
        public string Name 
        { 
            get; 
            private set; 
        }

        [JsonProperty]
        private readonly int Hash;

        private int NameHash()
        {
            object[] o = new object[] { Name, stages };
            return o.GetHashCode();
        }
        public bool Rename(string newName, int hash)
        {
            if (Hash.Equals(hash))
            {
                Name = newName;
                return true;
            }
            return false; 
        }

        public void Save()
        {
            ProjectIO.WriteProject(Runtime.Instance.LoadedProject, Metadata);
        }

       

        private Metadata Metadata
        {
            get
            {
                string projDir = Constants.ProjectsDir;
                string rootDir = Constants.WorkingRoot;
                string ext = Constants.ProjectFileExtension;
                string path = rootDir + projDir + '\\' + Name + ext;

                return new Metadata(Name, path, ext);
            }
        }
        /// <summary>
        /// Runs an import file dialog and when appropriately navigated to a project file, loads it.-
        /// </summary>
        /// <returns></returns>
        public static Project Load()
        {
            Project project = Default;
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

        public static Stage? GetStageByName(string stageName)
        {
            bool gotAsset = AssetLibrary.Fetch(stageName, out Stage stage);
            
            if (gotAsset) return stage; 
            return null; 
        }

        public void AddStage(Stage stage)
        {
            // makes sure the metadata is up-to-date.
            stage.Sync();

            stagesMeta.Add(stage.Metadata);
            stages.Add(stage);
        }

        /// <summary>
        /// use this for new projects and overwrite the default stage data, this prevents lockups
        /// </summary>
        /// <param name="name"></param>
        public Project(string name)
        {
            Name = name;
            stages = new List<Stage>();
            Hash = NameHash();
        }
        public static Project Default
        {
            get
            {
                Project defaultProj = new("Default project");
                defaultProj.AddStage(Stage.Default());
                return defaultProj;
            }
        }

        public Project()
        {

        }
        [JsonConstructor]
        public Project(List<Metadata>? stage_meta, string name, int hash)
        {
            this.stagesMeta = stage_meta;
            this.stages = new(); 
            this.Hash = hash;
            Name = name;
        }
    }
}