using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace pixel_renderer.IO
{
    public class ProjectIO
    {
        static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All
        };
        public static string Path => Constants.WorkingRoot + Constants.ProjectsDir;
        public static Project ReadProjectFile(string fileName)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                throw new Exception($"Path not found, A Directory was created at {Path}. please try placing data within this directory and try the operation again.");
            }
            var jsonSerializer = JsonSerializer.Create(settings);
            StreamReader reader = new(Path + "\\" + fileName + Constants.ProjectFileExtension);
            Project project = new(fileName);
            using JsonTextReader json = new(reader);
            project = jsonSerializer.Deserialize<Project>(json);

            if (project is null)
                throw new NullReferenceException();
            return project;
        }
        public static void SaveProject(Project project)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            if (File.Exists(Path + "/" + project.Name + Constants.ProjectFileExtension))
            {
                var overwriteWarningResult = MessageBox.Show($"Are you sure you want to overwrite Project {project.Name}.pxpj ? \n found at {Path}",
                                        "", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning,
                                         MessageBoxResult.No, MessageBoxOptions.RtlReading);
                if (overwriteWarningResult != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            using TextWriter writer = new StreamWriter(Path + "/" + project.Name + Constants.ProjectFileExtension);

            var jsonSerializer = JsonSerializer.Create(settings);

            jsonSerializer.Serialize(writer, project);
            writer.Close();
        }
        public static void TryFetchProject(out Project? outObject, string name)
        {
            outObject = new("Null");
            Project project = ReadProjectFile(name);
            if (project is null) return;
            outObject = project;
            return;
        }
    }
}