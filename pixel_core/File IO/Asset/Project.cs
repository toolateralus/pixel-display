﻿using Newtonsoft.Json;
using pixel_core.Assets;
using pixel_core.FileIO;
using pixel_core.Statics;
using System.Collections.Generic;
using System.IO;

namespace pixel_core
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project
    {
        public List<Stage> stages = new();
        [JsonProperty]
        public Settings settings = new();
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
                Interop.SetStage(stage);
            else Interop.InstantiateDefaultStageIntoProject();
        }
        public void Save()
        {
            ProjectIO.WriteProject(Interop.Project, Metadata);
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
            stagesMeta.Add(stage.Metadata);
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
            stages = new List<Stage>();
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