﻿using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.Assets
{
    public class Importer
    {
        public const int maxDepth = 100;
        /// <summary>
        /// Enumerates through all files in the Asset Import path and attempts to register them to the runtime AssetLibrary instance. 
        /// </summary>
        public static void Import(bool showMessage = false)
        {
            Interop.OnImport?.Invoke(); 
            
            var e = new EditorEvent(EditorEventFlags.FILE_VIEWER_UPDATE);
            Interop.RaiseInspectorEvent(e);

            Library.Dispose();
            ImportRecursively(Constants.WorkingRoot, 0);
        }
        private static List<Metadata> Import(string directory, string ext)
        {
            List<Metadata> collection = new();

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            GetFiles(directory, ext, collection);

            return collection;
        }
        private static void GetFiles(string directory, string ext, List<Metadata> collection)
        {
            var files = Directory.GetFiles(directory, $"*{ext}");
            foreach (var item in files)
            {
                var split = item.Split('\\');
                Metadata file = new(item);
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
            else
            {
                Interop.Log($"{depth} deep at import time.");
            }
        }
        private static void ImportAndRegister(string _dir)
        {
            var assets = Import(_dir, Constants.AssetsExt);
            var stages = Import(_dir, Constants.StagesExt);
            var bmps = Import(_dir, Constants.BmpExt);
            var pngs = Import(_dir, Constants.PngExt);
            var audioFiles = Import(_dir, Constants.Mp3Ext);
            var lua_scripts = Import(_dir, Constants.LuaExt);
            var pl_scripts = Import(_dir, Constants.PixelLangExt);

            foreach (var item in assets)
            {
                var asset = IO.ReadJson<Asset>(item);
                Library.Register(item, asset);
            }
            foreach (var stage in stages)
            {
                var asset = IO.ReadJson<Stage>(stage);
                if (asset != null)
                    Interop.OnStageAddedToProject?.Invoke(asset);
            }
            foreach (var script in lua_scripts)
            {
                string text = IO.Read(script);
                Library.Register(script, text);
            }

            foreach (var script in pl_scripts)
            {
                string text = IO.Read(script);
                Library.Register(script, text);
            }

            // this hold all "assets" or files without pre-loaded data, which get stored with a null value and just point to the file.
            var other = bmps.Concat(pngs);
            other = other.Concat(audioFiles);

            foreach (var item in other)
                Library.Register(item, null);
        }
        public static void ImportAssetDialog()
        {
            Metadata metadata = FileDialog.ImportFileDialog();

            var isPathValid = System.IO.Path.IsPathFullyQualified(metadata.FullPath);

            if (!isPathValid)
                return;
        }
    }
}
