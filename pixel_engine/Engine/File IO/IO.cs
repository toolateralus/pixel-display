﻿using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace pixel_renderer
{
    public static class IO
    {
        public static bool Skipping = false;
        private static JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
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
                    // this is a really janky way to return null out of this;
                    return (T)Convert.ChangeType(null, typeof(T));
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
                throw new FileNotFoundException("File was not found at provided path, or had an unsupported file extension");
            }
            catch(Exception e)
            {
                Runtime.Log("File read exception occured : \n\t" + e.Message, true, false);
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
            Runtime.Log($"Data written to {meta.fullPath}");
            if (closeStreamWhenFinished)
            {
                writer.Close();
                return null;
            }
            return writer;

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
    public class StageIO
    {
        private static void FindOrCreateStagesDirectory()
        {
            if (!Directory.Exists(Constants.WorkingRoot + Constants.StagesDir))
                Directory.CreateDirectory(Constants.WorkingRoot + Constants.StagesDir);
        }
        public static void WriteStage(Stage stage)
        {
            FindOrCreateStagesDirectory();
            IO.WriteJson(stage, stage.Metadata);
        }

        public static Stage? ReadStage(Metadata meta)
        {
            FindOrCreateStagesDirectory();

            // this is an inevitability of using a reference type based metadata,
            // consider using a struct if it makes sense to structure it that way
            
            if (meta is null)
                return null;

            Stage? stage = IO.ReadJson<Stage>(meta);
            return stage;
        }
    }
}