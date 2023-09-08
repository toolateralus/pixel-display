using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;
using Assimp.Configs;
using Metadata = Pixel.FileIO.Metadata;
using Pixel.Types.Physics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Pixel.Assets
{
    public class MeshImporter
    {
        public static Collider NewMeshCollider(Metadata meta)
        {
            var poly = GetPolygonFromMesh(meta);
            Node node = new();
            var collider = node.AddComponent<Collider>();
            collider.SetModel(poly);
            return collider;
        }
        public static Polygon GetPolygonFromMesh(Metadata meta)
        {
            AssimpContext importer = new();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            Scene scene = importer.ImportFile(meta.FullPath, PostProcessPreset.TargetRealTimeMaximumQuality);
            Mesh mesh = scene.Meshes[0];

            if (scene.MeshCount == 0)
                throw new NullReferenceException($"No meshes were found in {meta.FullPath}");

            Vector2[] vectors = new Vector2[mesh.Vertices.Count + 1];

            for (int i = mesh.Vertices.Count - 1; i >= 0; i--)
            {
                Vector3D vertex = mesh.Vertices[i];
                vectors[i] = new(vertex.X, vertex.Y);
            }

            return new Polygon(vectors);
        }

    }

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
            var obj_meshes = Import(_dir, ".obj");

            foreach (var item in assets)
            {
                if (IO.ReadJson<Asset>(item) is Asset asset)
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
            other = other.Concat(obj_meshes);

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
