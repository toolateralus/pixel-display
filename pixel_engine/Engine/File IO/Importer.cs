﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pixel_renderer.FileIO;

namespace pixel_renderer.Assets
{
    public class Importer
    {
        public const int maxDepth = 100; 
        /// <summary>
        /// Enumerates through all files in the Asset Import path and attempts to register them to the runtime AssetLibrary instance. 
        /// </summary>
        public static void Import(bool showMessage = false)
        {
            var e = new EditorEvent(EditorEventFlags.FILE_VIEWER_UPDATE);
            Runtime.RaiseInspectorEvent(e);
            AssetLibrary.Dispose();
            ImportRecursively(Constants.WorkingRoot, 0);
        }
        private static List<Metadata> Import(string directory, string ext)
        {

            List<Metadata> collection = new();

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (Constants.ReadableExtensions.Contains(ext))
                GetFiles(directory, ext, collection);

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
        private static void ImportRecursively(string dir, int depth)
        {
            if (depth > maxDepth)
                return;

            var dirs = Directory.GetDirectories(dir);
            ImportAndRegister(dir);

            if (dirs.Length > 0)
                foreach (var sub_dir in dirs)
                    ImportRecursively(sub_dir, depth++);
            else {
                Runtime.Log($"{depth} deep at import time.");
            }
        }
        private static void ImportAndRegister(string _dir)
        {
            var assets = Import(_dir, ".asset");
            var bmps = Import(_dir, ".bmp");
            var pngs = Import(_dir, ".png");
            var audioFiles = Import(_dir, ".mp3");
            var lua_scripts = Import(_dir, ".lua");

            foreach (var item in assets)
            {
                var asset = IO.ReadJson<Asset>(item);
                AssetLibrary.Register(item, asset);
            }

            foreach (var script in lua_scripts)
            {
                string text = IO.Read(script);
                AssetLibrary.Register(script, text);
            }
            // this hold all "assets" or files without pre-loaded data, which get stored with a null value and just point to the file.
            var other = bmps.Concat(pngs);
            other = other.Concat(audioFiles);

            foreach (var item in other)
                AssetLibrary.Register(item, null);
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
