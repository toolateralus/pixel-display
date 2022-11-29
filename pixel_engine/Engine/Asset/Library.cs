﻿using System;
using System.Collections.Generic;
using System.Linq;
using pixel_renderer.IO;

namespace pixel_renderer.Assets
{
    public class Library
    {
        public static Dictionary<Type, List<Asset>> LoadedAssets = new();
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
            if (LoadedAssets.TryGetValue(typeof(T), out List<Asset> found))
            {
                foreach (var _asset in found)
                {
                    if (_asset is null) continue;
                    result = _asset as T;


                }
                return true;
            }
            return false;
        }
        public static bool Fetch<T>(string name, out T result) where T : Asset
        {
            result = null;
            if (LoadedAssets.TryGetValue(typeof(T), out var found))
            {
                result = (T)found.Where(x => x.Name.Equals(name));
                return true;
            }
            return false;
        }
        public static bool Fetch<T>(out List<object> output)
        {
            output = new List<object>();
            foreach (var pair in from pair in LoadedAssets
                                 let type = pair.Key
                                 where type == typeof(T)
                                 select pair)
            {
                output.AddRange(from asset in pair.Value
                                select asset);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Save the currently loaded asset Library to the disk.
        /// </summary>
        public static void Sync()
        {
            var library = Clone();

            if (library is null) return;

            AssetIO.skippingOperation = false;

            foreach (var asset in library)
                AssetIO.SaveAsset(asset, asset.Name);
        }
        /// <summary>
        /// Clone the current Asset Library into a List.
        /// </summary>
        /// <returns>a clone of the currently loaded Assets library in a one dimensional list.</returns>
        public static List<Asset>? Clone()
        {
            List<Asset> library = new();

            foreach (var key in LoadedAssets)
                foreach (var item in key.Value)
                    library.Add(item);

            return library;
        }
        public static void Register(Type type, Asset asset)
        {
            if (!LoadedAssets.ContainsKey(type))
                LoadedAssets.Add(type, new List<Asset>());

            LoadedAssets[type].Add(asset);
        }
        public static void Unregister(Type type, string Name)
        {
            foreach (var asset in from asset in LoadedAssets[type]
                                  where asset.Name.Equals(Name)
                                  select asset)
            {
                LoadedAssets[type].Remove(asset);
            }
        }
    }
}
