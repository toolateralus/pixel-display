using Pixel.Statics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.FileIO
{
    public class AssetIO
    {
        public static string Path => Constants.WorkingRoot + Constants.AssetsDir;
        internal static void FindOrCreateAssetsDirectory()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }

        /// <summary>
        /// Checks for the existence of the Assets directory and if it exists, tries to read from the location of the data specified in the metadata object, then registers it to the AssetLibrary..
        /// </summary>
        /// <param name="meta"></param>
        /// <returns>Asset if found, else null </returns>
        public static void WriteAsset(Asset data, Metadata meta)
        {
            FindOrCreateAssetsDirectory();
            IO.WriteJson(data, meta);
        }

        static Dictionary<string, int> filesWritten = new();

        public static bool FindOrCreatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return false;
            }
            return true;
        }

        internal static void GuaranteeUniqueName(Metadata meta, Asset asset)
        {
            GetDir(meta, out string name, out string dir);

            _ = FindOrCreatePath(dir);

            name = DuplicateCheck(name, dir, asset);

            asset.Upload();
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

                    Metadata meta = new(isolated_name, path, extension);
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
        private static string DuplicateCheck(string fullName, string dir, Asset asset)
        {
            var fileNameSplit = fullName.Split(".").ToList();
            var extension = fileNameSplit.Last();
            fileNameSplit.RemoveAt(fileNameSplit.Count - 1);
            var name = string.Join('.', fileNameSplit);

            string fullPath = $"{dir}{fullName}";

            Metadata meta = new(name, fullPath, extension);

            Asset foundAsset = IO.ReadJson<Asset>(meta);

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
    }
}
