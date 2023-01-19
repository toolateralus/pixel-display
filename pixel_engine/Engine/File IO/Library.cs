﻿using System;
using System.Collections.Generic;
using pixel_renderer.FileIO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace pixel_renderer.Assets
{
    public class AssetLibrary
    {
        static Dictionary<Metadata, Asset> Current = new();
        internal static List<Metadata> LibraryMetadata() => Current.Keys.ToList(); 
        public static void Register((Metadata, Asset) assetPair)
        {
            Asset asset = assetPair.Item2;
            Metadata metadata = assetPair.Item1;

            if (Current.ContainsKey(metadata)) return;
            Current.Add(metadata, asset);
        }
        public static void Register(Metadata metadata, Asset asset)
        {
            if (Current.ContainsKey(metadata)) return;
            Current.Add(metadata, asset);
        }


        public static void Unregister(Metadata metadata) => Current.Remove(metadata);
        
        /// <summary>
        /// Try to retrieve Asset by UUID and Type@ ..\AppData\Assets\$path$
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// 
        public static bool Fetch<T>(out T result) where T : Asset
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
        public static bool Fetch<T>(out List<T> output) where T : Asset
        {

            output = new List<T>();
            foreach (var obj in Current.Values)
                if (obj.GetType() == typeof(T))
                    output.Add((T)obj);

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
        /// Attempts to retrieve metadata by asset file path (asset.pathFromRoot).
        /// </summary>
        /// <param name="asset"></param>
        /// <returns>Metadata if found, else null</returns>
        public static Metadata? FetchMeta(Asset asset)
        {
            return (Metadata?)(from _asset
                               in Current
                               where _asset.Value.Equals(asset)
                               select _asset.Value);
        }
        public static Metadata? FetchMeta(string name) 
        {
            return (Metadata?)(from asset
                               in Current
                               where asset.Value.Name.Equals(name)
                               select asset.Value);
        }

        /// <summary>
        /// Save the currently loaded asset Library to the disk.
        /// </summary>
        public static void Sync()
        {
            IO.Skipping = false;
            foreach (var pair in Current)
            {
                AssetIO.WriteAsset(new(pair.Value, pair.Key));
                WriteMetadata(pair);
            }
        }

        private static void WriteMetadata(KeyValuePair<Metadata, Asset> pair)
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
