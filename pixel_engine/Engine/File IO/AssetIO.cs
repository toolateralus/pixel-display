using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using pixel_renderer.Assets;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace pixel_renderer.FileIO
{
    public class AssetIO
    {
        internal static void FindOrCreateAssetsDirectory()
        {
            if (!Directory.Exists(IO.Path))
                Directory.CreateDirectory(IO.Path);
        }
        /// <summary>
        /// Checks for the existence of the Assets directory and if it exists, tries to read from the location of the data specified in the metadata object, then registers it to the AssetLibrary..
        /// </summary>
        /// <param name="meta"></param>
        /// <returns>Asset if found, else null </returns>
        public static void ReadAndRegister(out Asset? asset, Metadata meta)
        {
            asset = ReadAsset(meta);
            if (asset is null) 
                return;
            AssetLibrary.Register(meta, asset);
        }
        public static void WriteAsset((Asset, Metadata) pair)
        {
            FindOrCreateAssetsDirectory();
            var meta = pair.Item2;
            var data = pair.Item1;
            IO.WriteJson(data, meta);
            Runtime.Log($"Data written to {meta.fullPath} for {data.Name}");
        }
        public static Asset? ReadAsset(Metadata meta)
        {
            FindOrCreateAssetsDirectory();
            Asset asset = IO.ReadJson<Asset>(meta);
            return asset;
        }
    }
}
