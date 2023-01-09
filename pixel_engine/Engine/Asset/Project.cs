using Newtonsoft.Json;
using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.IO;
using System.Collections.Generic;

public class Project
{
    public int fileSize = 0;
    public List<Asset> library;
    public int stageIndex;
    public List<StageAsset> stages;
    public string Name { get; }
    
    public static Project LoadProject()
    {
        Project project = new("Default");
        FileDialog dlg = FileDialog.ImportFileDialog();

        if (dlg.type is null)
            return project;

        if (dlg.type.Equals(typeof(Project)))
            project = ProjectIO.ReadProjectFile(dlg.fileName);

        if (project is not null)
            return project;
        else return new("Default");
    }
    internal static string GetPathFromRoot(string filePath)
    {
        return filePath.Replace(Constants.AppDataDir + "\\Pixel", "");
    }

    /// <summary>
    /// use this for new projects and overwrite the default stage data, this prevents lockups
    /// </summary>
    /// <param name="name"></param>
    public Project(string name)
    {
        Name = name;
        library = Library.Clone();
        stageIndex = 0;
        fileSize = 0;
    }
    [JsonConstructor]
    public Project(List<StageAsset> stages, List<Asset> library, int stageIndex, int fileSize, string name)
    {
        this.stages = stages;
        this.library = library;
        this.stageIndex = stageIndex;
        this.fileSize = fileSize;
        Name = name;
    }
}