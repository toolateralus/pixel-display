﻿using System;
using System.Collections.Generic;
using pixel_renderer.FileIO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace pixel_renderer.Assets
{
    public class AssetLibrary
    {
        static Dictionary<Metadata, Asset> Current = new();
        internal static List<Metadata> LibraryMetadata() => Current.Keys.ToList(); 
        /// <summary>
        /// Registers an asset to the AssetLibrary.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="asset"></param>
        /// <returns>false if the asset was already in the library, and true if it was successfully added.</returns>
        public static bool Register(Metadata metadata, Asset asset)
        {
            if (Current.ContainsKey(metadata)) 
                return false;

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
                if (asset.GetType() == typeof(T))
                {
                    result = asset as T;
                    return true;
                }
            return false;
        }
        public static Metadata? FetchMetaRelative(string pathRelativeToRoot)
        {
            foreach (var asset in Current)
                if (asset.Value is null && asset.Key is not null && asset.Key.pathFromProjectRoot == pathRelativeToRoot)
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
        public static void Sync()
        {
            if (Runtime.Initialized)
            {
                RefreshStageMetadataWithinLoadedProject();
                Runtime.Current.project?.Save();
                foreach (var x in Runtime.Current.project.stages)
                {
                    x.Sync(); 
                    StageIO.WriteStage(x);
                }
            }
            

            foreach (KeyValuePair<Metadata, Asset> assetPair in Current)
            {
                if (assetPair.Value is null)
                    continue; 

                assetPair.Value.Sync();

                AssetIO.GuaranteeUniqueName(assetPair.Key, assetPair.Value);
                AssetIO.WriteAsset(assetPair.Value, assetPair.Key);
            }
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
        public static List<Asset>? Clone()
        {
            List<Asset> library = new();
            
            foreach (var pair in Current)
                    library.Add(pair.Value);

            return library;
        }
    }
}
