using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using pixel_renderer.Assets;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Documents.DocumentStructures;

namespace pixel_renderer.FileIO
{
    public class AssetIO
    {
        public static string Path => Constants.WorkingRoot + Constants.AssetsDir;
        internal static void FindOrCreateAssetsDirectory()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }

        public static void WriteMetadata(KeyValuePair<Metadata, Asset> pair)
        {
            string fullPath = new(pair.Key.fullPath.ToCharArray());
            string extension = new(pair.Key.extension.ToCharArray());

            string name = new(pair.Key.Name);

            string thisPath = fullPath;
            string thisExt = Constants.MetadataFileExtension;

            Metadata meta = new(name, fullPath, extension);
            Metadata this_meta = new(name, fullPath, extension);


            if (thisPath.Contains(meta.extension)
                && meta.extension != thisExt)
            {
                thisPath = thisPath.Replace(meta.extension, "");

                if (thisPath.Contains(thisExt))
                    thisPath = thisPath.Replace(Constants.MetadataFileExtension, "");

                thisPath += thisExt;
                this_meta.fullPath = thisPath;
                this_meta.pathFromProjectRoot = Project.GetPathFromRoot(thisPath);
                this_meta.extension = thisExt;
            }
            IO.WriteJson(meta, this_meta);
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
            string[] split;
            string name, dir;

            GetDir(meta, out split, out name, out dir);

            _ = FindOrCreatePath(dir);

            name = DuplicateCheck(name, dir, asset);

            asset.Name = name;
            meta.Name = name;

            UpdateMetadataPath(meta, name);

        }
        public static void GetDir(Metadata meta, out string[] split, out string name, out string dir)
            {
                split = meta.fullPath.Split("\\");

                // nullifies C:\\ cuz for some reason it would double up when reconstructing from array
                split[0] = "";

                name = split[^1];
                dir = meta.fullPath.Replace(name, "");
            }

        private static string DuplicateCheck(string name, string dir, Asset asset)
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
                    if (obj is Asset foundAsset && foundAsset == asset)
                        continue;

                    if (isolated_name == "")
                        isolated_name = "NamelessAsset";
                    if (filesWritten.ContainsKey(name))
                    {
                        isolated_name += filesWritten[name]++;
                        name = isolated_name + "." + extension;
                    }
                    else filesWritten.Add(name, 1);

                    Runtime.Log($"Number added to file {name}");
                }
            }
            return name;
        }

        private static void UpdateMetadataPath(Metadata meta, string name)
        {
           var path = meta.pathFromProjectRoot.Split("\\");
           meta.fullPath = Constants.WorkingRoot + path[0] + "\\" + path [1] + "\\" + name;
           meta.pathFromProjectRoot = Project.GetPathFromRoot(meta.fullPath);

        }
    }
}
