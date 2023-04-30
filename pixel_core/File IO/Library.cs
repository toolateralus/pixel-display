using Pixel.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Assets
{
    public class Library
    {
        static Dictionary<Metadata, object> Current = new();
        public static bool Busy { get; private set; }
        public static bool Register(ref Metadata metadata, Asset asset)
        {
            if (Busy)
                return false;
            
            CheckForDuplicates(ref metadata);

            if (Current.ContainsKey(metadata))
            {
                Current[metadata] = asset;
                return true;
            }
            Current.Add(metadata, asset);
            return true;
        }
        public static bool Register(Metadata metadata, Asset asset)
        {
            if (Busy)
                return false;

            CheckForDuplicates(ref metadata);

            if (Current.ContainsKey(metadata))
            {
                Current[metadata] = asset;
                return true;
            }
            Current.Add(metadata, asset);
            return true;
        }

        public static void CheckForDuplicates(ref Metadata meta)
        {
            foreach (var item in Current)
                if (item.Key.Path == meta.Path)
                    meta = item.Key;
        }
        public static bool Register(ref Metadata metadata, object asset)
        {
            if (Busy)
                return false;

            CheckForDuplicates(ref metadata);

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
            CheckForDuplicates(ref metadata);

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
        public static bool FetchFirstOfType<T>(out T? result) where T : Asset
        {
            result = null;
            foreach (var asset in Current.Values)
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

            if (output.Count > 0) return true;
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
        public static void SaveAll()
        {
            Busy = true;
            lock (Current)
            {
                if (Interop.Initialized)
                {
                    RefreshStageMetadataWithinLoadedProject();
                    Interop.Project?.Save();

                    foreach (var x in Interop.Project.stages)
                    {
                        x.Sync();
                        x.Save();
                    }
                }
                foreach (KeyValuePair<Metadata, object> assetPair in Current)
                {
                    if (assetPair.Value is null || assetPair.Value is not Asset a || a.Name == "NULL")
                        continue;

                    a.Metadata = assetPair.Key;

                    a.Sync();
                    a.Save();
                }
            }
            Busy = false;
        }
        private static void RefreshStageMetadataWithinLoadedProject()
        {
            if (Interop.Project == null)
                return;

            var stages = Interop.Project.stages;

            if (stages is null)
                return;

            foreach (var stage in stages)
            {
                var stages_meta = Interop.Project.stagesMeta;

                if (!stages_meta.Contains(stage.Metadata))
                    stages_meta.Add(stage.Metadata);
            }
        }
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
        internal static void Dispose()
        {
            Current.Clear();
        }
    }
}
