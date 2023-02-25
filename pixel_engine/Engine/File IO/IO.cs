using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace pixel_renderer
{
    public enum PixelDirectory
    {
        Root = 0,
        Projects = 1,
        Stages = 2,
        Assets = 3,
    }
    public static class IO
    {
        private static JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        
        /// <summary>
        /// this does not check if the directory or file exists, just deserializes a file into a json object of specified type and returns as C# object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="meta"></param>
        /// <param name="closeStreamWhenFinished"></param>
        /// <returns></returns>
        public static T? ReadJson<T>(Metadata meta, bool closeStreamWhenFinished = true) where T : new()
        {
            T? obj = new();
            try
            {

                if (meta is null)
                {
                    Runtime.Log(nameof(meta) + " was not found.");
                    return default;
                }
                if (Constants.ReadableExtensions.Contains(meta.extension))
                {
                    var jsonSerializer = JsonSerializer.Create(Settings);
                    StreamReader reader = new(meta.fullPath);
                    using JsonTextReader jsonReader = new(reader);
                    obj = jsonSerializer.Deserialize<T>(jsonReader);
                    if (closeStreamWhenFinished)
                        reader.Close();
                    return obj;
                }
                throw new FileNotFoundException($"JSON file was not found at provided path, or had an unsupported file extension \n Path: {meta.fullPath} \n Extension: {meta.extension}");
            }
            catch(Exception e)
            {
                Runtime.Log("JSON Read Failed : \n" + e.Message, true, false);
            }
            return obj;
        }
        /// <summary>
        /// this does not check if the directory exists nor does it instantiate one where it doesnt exist
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns>null if the writer is closed, and the writer if it's still open</returns>
        public static TextWriter? WriteJson<T>(T data, Metadata meta, bool closeStreamWhenFinished = true)
        {
            
            using TextWriter writer = new StreamWriter(meta.fullPath);
            var jsonSerializer = JsonSerializer.Create(Settings);
            jsonSerializer.Serialize(writer, data);
            Runtime.Log($"Data: typeof({data.GetType()}) written to Path: {meta.fullPath}");
            if (closeStreamWhenFinished)
            {
                writer.Close();
                return null;
            }
            return writer;

        }

        public static void Write<T>(T data, Metadata meta)
        {
            using StreamWriter writer = new(meta.fullPath);
            writer.Write(data);
            writer.Close();
        }
        public static string Read<T>(Metadata meta)
        {
            using StreamReader writer = new(meta.fullPath);
            string data = writer.ReadToEnd();
            writer.Close();
            return data; 
        }

        public static Bitmap ReadBitmap(Metadata meta)
        {
            var obj = IO.ReadJson<object>(meta);
            
            if (obj is Bitmap bitmap)
                return bitmap;

            return new(512,512);
        }

        public static MessageBoxResult FileOverrideWarning(string path)
        {
            return MessageBox.Show($"Are you sure you want to overwrite {path}?",
                                                        "", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning,
                                                         MessageBoxResult.No, MessageBoxOptions.RtlReading);
        }
        public static MessageBoxResult DoForAllQuestion()
        {
            return MessageBox.Show($"Do for all (uses last choice)",
                                                        "", MessageBoxButton.YesNo, MessageBoxImage.Question,
                                                         MessageBoxResult.No, MessageBoxOptions.RtlReading);
        }

    }
    public class ProjectIO
    {
        public static string root = Constants.WorkingRoot;
        public const string directory = Constants.ProjectsDir;
        public const string extension = Constants.ProjectFileExtension;
        public static string Path => root + directory;
        private static void FindOrCreateProjectsDirectory()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }
        public static void WriteProject(Project? proj, Metadata meta)
        {
            FindOrCreateProjectsDirectory();
            IO.WriteJson(proj, meta);
        }
        public static Project ReadProject(string name)
        {
            FindOrCreateProjectsDirectory();
            Metadata meta = new(name, Path + "\\" + name + extension, extension);
            var project = IO.ReadJson<Project>(meta);
            if (project is null) throw new FileNotFoundException(); 
            return project;  
        }
    }
}