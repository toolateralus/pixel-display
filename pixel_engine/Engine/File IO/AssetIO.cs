using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using pixel_renderer.Assets;
namespace pixel_renderer.IO
{
    public class AssetIO
    {
        public static bool skippingOperation = false;
        public static string Path => Settings.AppDataDir + Settings.AssetsDir; 
        public static void SaveAsset(Asset data, string fileName)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }

            if (File.Exists(Path + "/" + fileName + Settings.AssetsFileExtension))
            {
                if (!skippingOperation)
                {
                    var overwriteWarningResult = MessageBox.Show($"Are you sure you want to overwrite {fileName}.json ? \n found at {Path}",
                                            "", MessageBoxButton.YesNoCancel, MessageBoxImage.Error,
                                             MessageBoxResult.No, MessageBoxOptions.RtlReading);
                    var doForAllResult = MessageBox.Show($"Do for all (uses last choice)",
                                            "", MessageBoxButton.YesNo, MessageBoxImage.Error,
                                             MessageBoxResult.No, MessageBoxOptions.RtlReading);
                    if (doForAllResult == MessageBoxResult.Yes)
                    {
                        skippingOperation = true;
                    }
                    if (overwriteWarningResult != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }

            using TextWriter writer = new StreamWriter(Path + "/" + fileName + Settings.AssetsFileExtension);

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var jsonSerializer = JsonSerializer.Create(settings);

            jsonSerializer.Serialize(writer, data);

            writer.Close();
        }
        public static void TryDeserializeAssetFIle(ref Asset? outObject, string name)
        {
            Asset? _asset = ReadAssetFile(name);
            if (_asset is null) return;
            outObject = _asset;
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
            StreamReader reader = new(fileName);
            Asset asset = new("" + fileName, typeof(Asset), "");
            using JsonTextReader json = new(reader);
            try
            {
                asset = jsonSerializer.Deserialize<Asset>(json);
            }
            catch (Exception) { MessageBox.Show("File read error - Fked Up Big Time"); };
            return asset;
        }
        public static Asset? TryDeserializeNonAssetFile(string fileName, Type type, string assetName)
        {
            switch (type)
            {
                case var _ when type == typeof(Bitmap):

                    BitmapAsset bmpAsset = BitmapAsset.PathToAsset(fileName, assetName);
                    if (bmpAsset == null) return null;
                    bmpAsset.fileType = typeof(Bitmap);
                    return bmpAsset;

                case var _ when type == typeof(JsonObject):
                    var asset = ReadAssetFile(fileName);
                    return asset;


                default: return null;
            }
        }
    }

}
