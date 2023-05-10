using Newtonsoft.Json;
using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;

namespace Pixel
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
        static Dictionary<string, int> filesWritten = new();
        private static JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        internal static void GuaranteeUniqueName(Metadata meta, Asset asset)
        {
            IO.GetDir(meta, out string name, out string dir);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            name = DuplicateCheck(name, dir, asset);

            asset.Upload();
        }
        private static string DuplicateCheck(string fullName, string dir, Asset asset)
        {
            var fileNameSplit = fullName.Split(".").ToList();
            var extension = fileNameSplit.Last();
            fileNameSplit.RemoveAt(fileNameSplit.Count - 1);
            var name = string.Join('.', fileNameSplit);

            string fullPath = $"{dir}{fullName}";

            Metadata meta = new(fullPath);

            Asset foundAsset = ReadJson<Asset>(meta);

            if (File.Exists(fullPath) && foundAsset is not null && foundAsset.UUID == asset.UUID)
                return fullName;

            string nameWithoutNums = "";

            for (List<char> chars = name.ToList(); chars.Count > 0; chars.RemoveAt(chars.Count - 1))
            {
                if (!Constants.int_chars.Contains(chars.Last()))
                {
                    nameWithoutNums = string.Concat(chars);
                    break;
                }
            }

            List<int> duplicateNames = new();

            // TODO: Fix this! it keeps overflowing constantly

            foreach (var path in Directory.EnumerateFiles(dir))
            {
                var splitPath = path.Split("\\").Last().Split('.').ToList();
                if (splitPath.Last() != extension)
                    continue;
                splitPath.RemoveAt(splitPath.Count - 1);
                var fileName = string.Join('.', splitPath);

                List<char> numbers = new();
                for (List<char> chars = fileName.ToList(); chars.Count > 0; chars.RemoveAt(chars.Count - 1))
                {
                    if (!Constants.int_chars.Contains(chars.Last()))
                    {
                        if (string.Concat(chars) != nameWithoutNums)
                            break;
                        duplicateNames.Add(string.Concat(numbers).ToInt());
                        if (duplicateNames.Count >= 1000)
                            throw new FileNamingException($"There are too many files already named \"{nameWithoutNums}.{extension}\"");
                        break;
                    }
                    numbers.Insert(0, chars.Last());
                }
            }
            for (int i = 1; i < 1000; i++)
            {
                if (duplicateNames.Contains(i))
                    continue;
                Interop.Log($"Number {i} added to file \"{nameWithoutNums}.{extension}\"");
                return $"{nameWithoutNums}{i}.{extension}";
            }
            throw new Exception("Unknown Exception");
        }
        public static string GetDirectory(PixelDirectory dir)
        {
            return dir switch
            {
                PixelDirectory.Root => Constants.WorkingRoot,
                PixelDirectory.Projects => Constants.ProjectsDir,
                PixelDirectory.Stages => Constants.StagesDir,
                PixelDirectory.Assets => Constants.AssetsDir,
                _ => throw new NotImplementedException(),
            };
        }
        public static T? ReadJson<T>(Metadata meta)
        {
            T? obj = default;

            if (meta is null)
                return obj;

            if (!File.Exists(meta.Path))
                return obj;

            try
            {
                if (meta is null)
                {
                    Interop.Log("Metadata was not found.");
                    return obj;
                }
                if (Constants.ReadableExtensions.Contains(meta.extension))
                {
                    var jsonSerializer = JsonSerializer.Create(Settings);

                    StreamReader reader = new(meta.Path);

                    using JsonTextReader jsonReader = new(reader);

                    obj = jsonSerializer.Deserialize<T>(jsonReader);

                    return obj;
                }
            }
            catch (Exception e)
            {
                if (!(Interop.IsRunning && Interop.Initialized))
                    throw;
                Interop.Log("JSON Read Failed : \n" + e.Message);
            }
            return obj;
        }
        public static TextWriter? WriteJson<T>(T data, Metadata meta, bool closeStreamWhenFinished = true)
        {

            using TextWriter writer = new StreamWriter(meta.Path);
            var jsonSerializer = JsonSerializer.Create(Settings);
            jsonSerializer.Serialize(writer, data);
            Interop.Log($"{data.GetType()} written at: {meta.Path}");
            if (closeStreamWhenFinished)
            {
                writer.Close();
                return null;
            }
            return writer;

        }
        public static string? WriteJsonToString<T>(T data)
        {
            using TextWriter writer = new StringWriter();
            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Serialize(writer, data);
            var output = writer.ToString();
            writer.Close();
            return output;
        }
        public static void Write(object data, Metadata meta)
        {
            using StreamWriter writer = new(meta.Path);
            writer.Write(data);
            writer.Close();
        }
        public static string Read(Metadata meta)
        {
            using StreamReader writer = new(meta.Path);
            string data = writer.ReadToEnd();
            writer.Close();
            return data;
        }
        public static void GetDir(Metadata meta, out string name, out string dir)
        {
            var split = meta.Path.Split('\\');

            // nullifies C:\\ cuz for some reason it would double up when reconstructing from array
            split[0] = "";

            name = split[^1];
            dir = meta.Path.Replace(name, "");
        }
        public static string DuplicateCheck(string name, string dir)
        {
            foreach (var path in Directory.EnumerateFiles(dir))
            {
                var splitPath = path.Split("\\");
                var fileName = splitPath[^1];

                if (fileName == name)
                {
                    var fileNameSplit = name.Split(".");
                    var isolated_name = fileNameSplit[0];
                    var extension = fileNameSplit[1];

                    // this metadata is created to check if there are any existing files in place of this one

                    Metadata meta = new(path);
                    object obj = IO.ReadJson<object>(meta);

                    // this allows us to overwrite files that have already been
                    // read and or written this session by comparing their data


                    if (isolated_name == "")
                        isolated_name = "NamelessAsset";

                    if (filesWritten.ContainsKey(name))
                    {
                        isolated_name += filesWritten[name]++;
                        name = isolated_name + "." + extension;
                    }
                    else filesWritten.Add(name, 1);

                    Interop.Log($"Number added to file {name}");
                }


            }
            return name;
        }
    }
}