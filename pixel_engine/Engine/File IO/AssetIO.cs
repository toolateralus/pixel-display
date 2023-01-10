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

namespace pixel_renderer.IO
{
    public class AssetIO
    {
        public static bool Skipping = false;
        public static string Path => Constants.WorkingRoot + Constants.AssetsDir; 

        /// <summary>
        /// this does not check if the directory exists nor does it instantiate one where it doesnt exist
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns>null if the writer is closed, and the writer if it's still open</returns>
        public static TextWriter? WriteFile<T>(T data, Metadata meta, bool closeStreamWhenFinished = true)
        {
            using TextWriter writer = new StreamWriter(meta.fullPath);
            var jsonSerializer = JsonSerializer.Create(Settings);
            jsonSerializer.Serialize(writer, data);
            if (closeStreamWhenFinished)
            {
                writer.Close();
                return null; 
            }
            return writer; 
        }
        /// <summary>
        /// this does not check if the directory or file exists, just deserializes a file into a json object of specified type and returns as C# object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="meta"></param>
        /// <param name="closeStreamWhenFinished"></param>
        /// <returns></returns>
        public static T ReadFile<T>(Metadata meta, bool closeStreamWhenFinished = true)
        {
            var jsonSerializer = JsonSerializer.Create(Settings);
            StreamReader reader = new(meta.fullPath);
            using JsonTextReader jsonReader = new(reader);
            T obj = jsonSerializer.Deserialize<T>(jsonReader);
            if (closeStreamWhenFinished) 
                reader.Close();
            return obj;
        }

        public static void SaveAsset((Asset, Metadata) pair)
        {
            FindOrCreateAssetsDirectory();
            
            var meta = pair.Item2;
            var data = pair.Item1;
            
            if (!File.Exists(meta.fullPath)) 
                throw new FileNotFoundException(meta.fullPath);

            if (!Skipping)
            {
                MessageBoxResult overwriteWarningResult = YesNoCancelWarning(meta.Name);
                MessageBoxResult doForAllResult = YesNoQuestion();

                if (doForAllResult == MessageBoxResult.Yes)
                    Skipping = true;
                if (overwriteWarningResult != MessageBoxResult.Yes)
                    return;
            }

            WriteFile(data, meta);
        }
        private static void FindOrCreateAssetsDirectory()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }
        private static MessageBoxResult YesNoQuestion()
        {
            return MessageBox.Show($"Do for all (uses last choice)",
                                                        "", MessageBoxButton.YesNo, MessageBoxImage.Question,
                                                         MessageBoxResult.No, MessageBoxOptions.RtlReading);
        }
        private static MessageBoxResult YesNoCancelWarning(string fileName)
        {
            return MessageBox.Show($"Are you sure you want to overwrite {fileName}.json ? \n found at {Path}",
                                                        "", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning,
                                                         MessageBoxResult.No, MessageBoxOptions.RtlReading);
        }
        public static void TryDeserializeAssetFile(out Asset? outObject, Metadata meta)
        {
            Asset? _asset = ReadAssetFile(meta);
            outObject = _asset;
            if (_asset is null) return;
            Library.Register(_asset.GetType(), _asset);
            return;
        }
        private static JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
        };
        /// <summary>
        /// Checks for the existence of the Assets directory and if it exists, tries to read from the location of the data specified in the metadata object.
        /// </summary>
        /// <param name="meta"></param>
        /// <returns>Asset if found, else null </returns>
        public static Asset? ReadAssetFile(Metadata meta)
        {
            FindOrCreateAssetsDirectory();
            Asset asset = ReadFile<Asset>(meta);
            return asset;
        }
    }
}
