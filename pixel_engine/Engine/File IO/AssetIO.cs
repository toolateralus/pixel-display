using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using pixel_renderer.Assets;
using System.Collections.Generic;
using System.Linq;

namespace pixel_renderer.IO
{
    public class AssetIO
    {
        public static bool Skipping = false;
        public static string Path => Constants.WorkingRoot + Constants.AssetsDir; 
        public static void SaveAsset(Asset data, string fileName)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            if (File.Exists(Path + "\\" + fileName + Constants.AssetsFileExtension))
            {
                if (!Skipping)
                {
                    var overwriteWarningResult = MessageBox.Show($"Are you sure you want to overwrite {fileName}.json ? \n found at {Path}",
                                            "", MessageBoxButton.YesNoCancel, MessageBoxImage.Error,
                                             MessageBoxResult.No, MessageBoxOptions.RtlReading);
                    var doForAllResult = MessageBox.Show($"Do for all (uses last choice)",
                                            "", MessageBoxButton.YesNo, MessageBoxImage.Error,
                                             MessageBoxResult.No, MessageBoxOptions.RtlReading);
                    if (doForAllResult == MessageBoxResult.Yes)
                    {
                        Skipping = true;
                    }
                    if (overwriteWarningResult != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }

            using TextWriter writer = new StreamWriter(Path + "\\" + fileName + Constants.AssetsFileExtension);

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var jsonSerializer = JsonSerializer.Create(settings);

            jsonSerializer.Serialize(writer, data);

            writer.Close();
        }
        public static void TryDeserializeAssetFile(out Asset? outObject, string name)
        {
            Asset? _asset = ReadAssetFile(name);
            outObject = _asset;
            if (_asset is null) return;
            Library.Register(_asset.GetType(), _asset);
            return;
        }
        public static Asset? ReadAssetFile(string fileName)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                return null;
            }
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            var jsonSerializer = JsonSerializer.Create(settings);
            StreamReader reader = new(Path + "\\" + fileName + Constants.AssetsFileExtension);
            Asset asset = new(fileName, typeof(Asset));
            using JsonTextReader json = new(reader);
            try
            {
                asset = jsonSerializer.Deserialize<Asset>(json);
            }
            catch (Exception e) {
                var x = e.Message.Take(100);    
                MessageBox.Show(x.ToString()); 
            
            };
            return asset;
        }
        public static Asset? TryDeserializeNonAssetFile(string newName, string extension, string fullPath)
        {

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                return null;
            }
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            var jsonSerializer = JsonSerializer.Create(settings);
            
            StreamReader reader = new(fullPath);

            Asset asset = new("New Asset", typeof(Asset));

            using JsonTextReader json = new(reader);

            try
            {
                asset = jsonSerializer.Deserialize<Asset>(json);
            }
            catch (Exception e)
            {
                var x = e.Message.Take(100);
                MessageBox.Show(x.ToString());

            };
            return asset;
        }
        

        
    }
}
