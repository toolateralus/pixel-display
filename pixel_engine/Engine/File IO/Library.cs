using System;
using System.Collections.Generic;
using pixel_renderer.FileIO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;
using System.Drawing;

namespace pixel_renderer.Assets
{
    public class AssetLibrary
    {
        static Dictionary<Metadata, object> Current = new();
        public static bool Busy { get; private set; }
        internal static List<Metadata> LibraryMetadata() => Current.Keys.ToList(); 
        /// <summary>
        /// Registers an asset to the AssetLibrary.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="asset"></param>
        /// <returns>false if the asset was already in the library, and true if it was successfully added.</returns>
        public static bool Register(Metadata metadata, Asset asset)
        {
            if (Busy) 
                return false;

            if (Current.ContainsKey(metadata))
            {
                Current[metadata] = asset;
                return true; 
            }

            Current.Add(metadata, asset);
            return true; 
        }
        public static bool Register(Metadata metadata, object value)
        {
            if (Busy)
                return false;

            if (Current.ContainsKey(metadata))
            {
                Current[metadata] = value;
                return true;
            }

            Current.Add(metadata, value);
            return true;
        }
        public static async Task<bool> RegisterAsync(Metadata metadata, Asset asset, int maxTryCount = 1000)
        {
            int tries = 0;
            while (Busy)
            {
                tries++;
                if (tries > maxTryCount)
                    return false; 
                await Task.Delay(10);
            }
            if (Current.ContainsKey(metadata))
            {
                Current[metadata] = asset;
                return true; 
            }
            Current.Add(metadata, asset);
            return true;
        }
        public static void Unregister(Metadata metadata) => Current.Remove(metadata);
        /// <summary>
        /// Try to retrieve Asset by UUID and Type@ ..\AppData\Assets\$path$
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// 
        public static bool Fetch<T>(out T? result) where T : Asset
        {
            result = null;
            foreach(var asset in Current.Values)
                if (asset != null && asset.GetType() == typeof(T))
                {
                    result = asset as T;
                    return true;
                }
            return false;
        }
        public static Metadata? FetchMetaRelative(string pathRelativeToRoot)
        {
            foreach (var asset in Current)
                if (asset.Key is not null && asset.Key.pathFromProjectRoot == pathRelativeToRoot)
                    return asset.Key;
            return default; 
                    
        }
        public static Metadata? FetchMeta(string name)
        {
            foreach (var asset in Current)
                if (asset.Value is null && asset.Key is not null && name == asset.Key.Name)
                    return asset.Key;
            return default; 
        }
        public static bool Fetch<T>(out List<T> output) where T : Asset
        {

            output = new List<T>();
            foreach (var obj in Current.Values)
                if (obj is not null && obj.GetType() == typeof(T))
                {
                    output.Add((T)obj);
                }

            if(output.Count > 0) return true;
            return false; 
        }
        public static bool Fetch<T>(string name, out T result) where T : Asset
        {
            Fetch<T>(out List<T> list);

            bool r = false;
            result = null;

            foreach (var item in list)
                if (item.Name == name)
                {
                    result = item;
                    r = true;
                    break; 
                }
            return r; 
        }
        /// <summary>
        /// Save the currently loaded asset Library and project to the disk.
        /// </summary>
        /// 
        public static void SaveAll()
        {
            Busy = true; 
            lock (Current)
            {
                if (Runtime.Initialized)
                {
                    RefreshStageMetadataWithinLoadedProject();
                    Runtime.Current.project?.Save();

                    foreach (var x in Runtime.Current.project.stages)
                    {
                        x.Sync();
                        x.Save(); 
                    }
                }
                foreach (KeyValuePair<Metadata, object> assetPair in Current)
                {
                    if (assetPair.Value is null || assetPair.Value is not Asset a)
                        continue; 

                    a.Sync();
                    a.Save();
                }
            }
            Busy = false; 
        }
        private static void RefreshStageMetadataWithinLoadedProject()
        {
            if (Runtime.Current.project == null)
                return;

            var stages = Runtime.Current.project.stages;
            
            if (stages is null) 
                return; 

            foreach (var stage in stages)
            {
                var stages_meta = Runtime.Current.project.stagesMeta;

                if (!stages_meta.Contains(stage.Metadata))
                    stages_meta.Add(stage.Metadata);
            }
        }
        /// <summary>
        /// Clone the current Asset Library into a List.
        /// </summary>
        /// <returns>a clone of the currently loaded Assets library in a one dimensional list.</returns>
        public static List<object>? Clone()
        {
            List<object> library = new();
            
            foreach (var pair in Current)
                    library.Add(pair.Value);

            return library;
        }
        public static List<Metadata> GetAllKeys()
        {
            return Current.Keys.ToList();  
        }
        /// <summary>
        /// note: when this finds an asset, it returns it. however,  due to the way image storage works, 
        /// if it finds an image it creates a Bitmap and returns that instead.
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static object FetchByMeta(Metadata meta)
        {
            if (meta is null)
                return null;

            var matches = Current.Where(p => p.Key.Path == meta.Path);
            var match = matches?.First();
            var asset = match.Value.Value;

            if (asset is null)
            {
                if (meta.extension == ".bmp" || meta.extension == ".png")
                {
                    Bitmap bmp = new(meta.Path);
                    return bmp; 
                }
                if (meta.extension == ".lua")
                {
                    string script = IO.Read(meta);
                    return script;
                }
            }

            return asset;

        }
    }
}
