using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using pixel_renderer.FileIO;
namespace pixel_renderer.Assets
{
    public class Importer
    {
        public static string DirectoryPath => Constants.WorkingRoot + Constants.AssetsDir;
        /// <summary>
        /// Enumerates through all files in the Asset Import path and attempts to register them to the runtime AssetLibrary instance. 
        /// </summary>
        public static void Import(bool showMessage = false)
        {
            ImportAll(Constants.WorkingRoot);
        }

        private static void ImportAssets()
        {
            foreach (var x in Import(Constants.WorkingRoot, Constants.AssetsFileExtension))
            {
                var asset = IO.ReadJson<Asset>(x);
                bool result = AssetLibrary.Register(x, asset);
                if (!result)
                    Runtime.Log($"Importer tried to register an asset that already been registered. Asset {asset.Name} \n {asset.GetType()} at path {x.Path}");
            }

        }

        private static void ImportBitmaps()
        {
             foreach (var x in Import(Constants.WorkingRoot + Constants.AssetsDir, Constants.PngExt))
                AssetLibrary.Register(x, null);

            foreach (var dir in Directory.GetDirectories(Constants.WorkingRoot + Constants.AssetsDir))
                foreach (var x in Import(dir, Constants.PngExt))
                    AssetLibrary.Register(x, null);
        }

        private static List<Metadata> Import(string directory, string ext)
        {

            List<Metadata> collection = new();

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (Constants.ReadableExtensions.Contains(ext))
            {
                GetFiles(directory, ext, collection);
            }
            return collection;
        }

        private static void GetFiles(string directory, string ext, List<Metadata> collection)
        {
            var files = Directory.GetFiles(directory, $"*{ext}");
            foreach (var item in files)
            {
                var split = item.Split('\\');
                var name = split[^1].Replace($"{ext}", "");
                Metadata file = new(name, item, ext);
                collection.Add(file);
            }
        }

        private static void ImportAll(string dir)
        {
            var dirs = Directory.GetDirectories(dir);
            var files = Directory.GetFiles(dir);
            
            foreach (var _dir in dirs)
            {
                var subDirs = Directory.GetDirectories(_dir);
                if (subDirs.Length > 0)
                    foreach (var __dir in subDirs)
                    {
                        var _subDirs = Directory.GetDirectories(__dir);
                        if (_subDirs.Length > 0)
                            foreach (var ___dir in _subDirs)
                            {
                                var __subDirs = Directory.GetDirectories(___dir);
                                if (__subDirs.Length > 0)
                                    foreach (var ____dir in __subDirs)
                                    {
                                        ImportAndRegister(____dir);
                                    }
                                ImportAndRegister(___dir);
                            }
                        ImportAndRegister(__dir);
                    }
                ImportAndRegister(_dir);
            }
            ImportAndRegister(dir);

        }

        private static void ImportAndRegister(string _dir)
        {
            var assets = Import(_dir, ".asset");
            var images = Import(_dir, ".bmp");
            foreach (var item in assets)
            {
                var asset = IO.ReadJson<Asset>(item);
                AssetLibrary.Register(item, asset);
            }
            foreach (var item in images)
                AssetLibrary.Register(item, null);
        }

        public static void GetFileNameAndExtensionFromPath(string path, out string name, out string ext)
        {
            var split = path.Split('\\');
            string nameAndExt = split[^1];
            split = nameAndExt.Split('.');
            name = split[0];
            ext = split[1];
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns>type if supported, else returns typeof(object) (generic) </returns>
        public static Type TypeFromExtension(string type)
        {
            if (type.Contains('.'))
                type = type.Remove('.');

            return type switch
            {
                Constants.AssetsFileExtension => typeof(Asset),
                Constants.ProjectFileExtension => typeof(Project),
                Constants.MetadataFileExtension => typeof(Metadata),
                Constants.PngExt => typeof(Bitmap),
                 _ => typeof(object),
            };
        }
        public static string ExtensionFromType(Type type)
        {
            if (type == typeof(Asset))
                return Constants.AssetsFileExtension;
            if (type == typeof(Project))
                return Constants.ProjectFileExtension;
            if (type == typeof(Bitmap))
                return Constants.PngExt;
            if (type == typeof(Metadata))
                return Constants.MetadataFileExtension;

            return "File Extension not found from type, either it's unsupported or was edited";
        }
        public static void ImportAssetDialog()
        {
            Metadata metadata = FileDialog.ImportFileDialog();

            var isPathValid = System.IO.Path.IsPathFullyQualified(metadata.Path);
            
            if (!isPathValid) 
                return;
        }
    }
}
