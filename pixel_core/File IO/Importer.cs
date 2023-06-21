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

namespace Pixel.Assets
{
    public class MeshImporter
    {
        public static Collider GenerateMeshCollider(Metadata meta)
        {
            AssimpContext importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            Scene scene = importer.ImportFile(meta.Path, PostProcessPreset.TargetRealTimeMaximumQuality);
            Mesh mesh = scene.Meshes[0];

            Node node = new();
            var collider = node.AddComponent<Collider>();

            if (scene.MeshCount == 0)
                return collider;

            Vector2[] vectors = new Vector2[mesh.Vertices.Count + 1];

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                Vector3D vertex = mesh.Vertices[i];
                vectors[i] = new(vertex.X, vertex.Y);
            }

            var polygon = new Polygon(vectors);
            collider.SetModel(polygon);
            return collider;
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
            var assets = Import(_dir, ".asset");
            var bmps = Import(_dir, ".bmp");
            var pngs = Import(_dir, ".png");
            var audioFiles = Import(_dir, ".mp3");
            var lua_scripts = Import(_dir, ".lua");
            var pl_scripts = Import(_dir, ".pl");

            foreach (var item in assets)
            {
                var asset = IO.ReadJson<Asset>(item);
                Library.Register(item, asset);
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

            var isPathValid = System.IO.Path.IsPathFullyQualified(metadata.Path);

            if (!isPathValid)
                return;
        }
    }
}
