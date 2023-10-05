using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Statics;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pixel
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project
    {
        public List<Stage> stages = new();
        [JsonProperty]
        public List<Metadata> stagesMeta = new();
        [JsonProperty]
        public string Name = "DefaultProjectName";

        public void TryLoadStage(int index)
        {
            if (stagesMeta.Count == 0 || stagesMeta.Count <= index)
            {
                Interop.Log("No stage found");
                Interop.InstantiateDefaultStageIntoProject();
                return;
            }

            if (IO.ReadJson<Stage>(stagesMeta[index]) is Stage stage)
                Interop.Stage = stage;
            else Interop.InstantiateDefaultStageIntoProject();
        }
        public void Save()
        {
            IO.Write(Interop.Project, Metadata);
        }
        private Metadata Metadata
        {
            get
            {
                string projDir = Constants.ProjectsDir;
                string rootDir = Constants.WorkingRoot;
          
                                
                string ext = Constants.ProjectFileExtension;
                string path = rootDir + projDir + '/' + Name + ext;
                
                return new Metadata(path);
            }
        }
       
        internal static string GetPathFromRoot(string filePath)
        {
            var output = filePath.Replace(Constants.WorkingRoot, "");
            return output;
        }
        public static Stage? GetStageByName(string stageName)
        {
            bool gotAsset = Library.Fetch(stageName, out Stage stage);
            if (gotAsset) return stage;
            return null;
        }
        public void AddStage(Stage stage)
        {
            stagesMeta.Add(stage.metadata);
            stages.Add(stage);

            if (stagesMeta.Count > 0)
                Constants.RemoveDuplicatesFromList(stagesMeta);
            if (stages.Count > 0)
                Constants.RemoveDuplicatesFromList(stages);
        }

        /// <summary>
        /// use this for new projects and overwrite the default stage data, this prevents lockups
        /// </summary>
        /// <param name="name"></param>
        public Project(string name)
        {
            Name = name;
        }
        public static Project Default => new("Default Project");
        public Project() { }
        [JsonConstructor]
        public Project(List<Metadata> stage_meta, string name, int hash)
        {
            this.stagesMeta = stage_meta;
            this.stages = new();
            Name = name;
        }
    }
}