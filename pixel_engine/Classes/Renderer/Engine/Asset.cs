using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Xml.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    /// <summary>
    /// Base class for all Pixel_engine Assets
    /// </summary>
    public class Asset
    {
        public string pathFromRoot = "";
        public string fileSize = "";
        private string _uuid = "";
        public string UUID { get { return _uuid; } init => _uuid = pixel_renderer.UUID.NewUUID(); }
        public string Name = "New Asset";
        public Type fileType;
        public Asset(string name, Type fileType)
        {
            Name = name;
            this.fileType = fileType; 
        }
    }
    public class BitmapAsset : Asset
    {
        public Bitmap? RuntimeValue = null; 
        public BitmapAsset(string name) : base(name, typeof(Bitmap))
        {

        }
        public static BitmapAsset BitmapToAsset(string fileName)
        {
            Bitmap? bmp = new(fileName);

            BitmapAsset asset = new("BitmapAsset");
            
            if (bmp != null)
                asset.RuntimeValue = bmp;
            
            return asset;
        }
        public Color[,] Colors
        {
            get  => ColorArrayFromBitmap(RuntimeValue);
        }
        public static Bitmap BitmapFromColorArray(Color[,] colors)
        {
            Bitmap bitmap = new(colors.GetLength(0) , colors.GetLength(1));  
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, colors[i, j]);
                }
            }
            return bitmap;
        }
        public static Color[,] ColorArrayFromBitmap(Bitmap bitmap)
        {
            Color[,] s = new Color[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    s[i, j] = bitmap.GetPixel(i, j);
                }
            }
            return s;
        }
    }
    public class FontAsset : Asset 
    {
        new public string Name = "New Font Asset";
        public Dictionary<char, Bitmap> characters = new();
        internal static List<Bitmap> GetCharacterImages(FontAsset asset, string text)
        {
            List<Bitmap> output = new();
            int i = 0;

            foreach (char character in text)
            {
                var x = character;
                if (char.IsLower(character))
                {
                    x = char.ToUpper(character);
                }
                var img = (Bitmap)asset.characters[x].Clone();
                output.Add(img);
                i++;
            }
            return output;
        }
        internal static List<Vec2> GetCharacterPosition(FontAsset asset)
        {
            List<Vec2> positions = new();
            foreach (var x in asset.characters.Values)
                positions.Add(new(x.Width, x.Height));

            return positions; 
        }
        public FontAsset(string name, Type fileType) : base(name, fileType)
        {
        }
    }
    public static class FontAssetFactory
    {
        public static FontAsset CreateFont(int start, int end, Bitmap[] characters)
        {
            FontAsset fontAsset = new($"fontAsset{start}", typeof(pixel_renderer.FontAsset));
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = start; i < end; i++)
            {
                if (i > alpha.Length || i > characters.Length)
                {
                    MessageBox.Show("Font asset could not be created, Index out of range.");
                    return null; 
                }
                fontAsset.characters.Add(alpha[i], characters[i]);
            }
            if (fontAsset.characters.Count <= 0)
            {
                MessageBox.Show("Font is empty.");
            }
            return fontAsset; 
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="text"></param>
        /// <returns> a List of Bitmap objects ordered by their Character value in accordance to the Text passed in.</returns>
        internal static void InitializeDefaultFont()
        {
            var path = Settings.Appdata + Settings.FontDirectory;

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            IEnumerable<string> files = Directory.GetFiles(path);

            int i = 0; 

            foreach (string file in files)
            {
                BitmapAsset bitmap = new($"{'a' + i}")
                {
                    RuntimeValue = new(file),
                    fileType = typeof(Bitmap)
                };
                AssetLibrary.Register(typeof(BitmapAsset), bitmap);
                i++;
            }
        }
    }
    public static class AssetIO
    {
        public static bool skippingOperation = false; 
        public static string Path => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Pixel\\Assets";
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

            using TextWriter writer = new StreamWriter(Path + "/" + fileName + ".json");

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var jsonSerializer = JsonSerializer.Create(settings);
            
            jsonSerializer.Serialize(writer, data);
            
            writer.Close();
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

            Asset asset = new("" + fileName, typeof(Asset)); 

            using JsonTextReader json = new(reader);

            try
            {
                asset = jsonSerializer.Deserialize<Asset>(json) ?? null;
            }
            catch (Exception e) { MessageBox.Show("File read error - Fked Up Big Time"); };
            asset.Name += asset.fileType ?? null; 
            return asset;
        }
        public static Asset? TryDeserializeNonAssetFile(string fileName, Type type)
        {
            switch (type)
            {
                case var _ when type == typeof(Bitmap):

                    BitmapAsset bmpAsset = BitmapAsset.BitmapToAsset(fileName);
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
    public static class AssetPipeline
    {
        public static string Path => Settings.Appdata + Settings.AssetsDirectory;
        /// <summary>
        /// Enumerates through all files in the Asset Import path and attempts to register them to the runtime AssetLibrary instance. 
        /// </summary>
        public static async Task ImportAsync(bool showMessage = false)
        {
            if (Runtime.Instance.IsRunning)
                if (showMessage)
                {
                    var msg = MessageBox.Show("Pixel needs to Import: Press OK to start.", "Asset Importer", MessageBoxButton.OKCancel);
                    if (msg == MessageBoxResult.Cancel)
                        return;
                }

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            await Task.Run(ImportTask);

            if (!Runtime.Instance.IsRunning)
                if (showMessage)
                {
                    var syncResult = MessageBox
                        .Show("Import complete! Do you want to sync?",
                        "Asset Importer",
                        MessageBoxButton.YesNo);

                    if (syncResult == MessageBoxResult.Yes) AssetLibrary.Sync();
                }
        }
        private static void ImportTask()
        {
            foreach (var dir in Directory.GetDirectories(Path))
                foreach (var file in Directory.GetFiles(dir))
                {
                    var typeString = file.Split('.')[1];

                    var typeRef = TypeFromExtension(typeString);

                    var asset = TryPullObject(file, typeRef);

                    if (asset is not null)
                    {
                        if (asset.fileType is null)
                            asset.fileType = typeof(Asset);

                        AssetLibrary.Register(asset.fileType, asset);
                    }
                };
        }
        /// <summary>
        /// Read and deserialize a single file from Path"
        /// </summary>
        /// <param name="path"> the file path that will be read from ie. C:\\User\\AppData\\Pixel\\ProjectA\\Asssets\\heanti.gif</param>
        /// <returns>Asset if it exists at path, else null.</returns>
        public static Asset? TryPullObject(string path, Type type)
        {
            if (!File.Exists(path)) return null; 
            return AssetIO.TryDeserializeNonAssetFile(path, type);
        }
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns>type if supported, else returns typeof(object) (generic) </returns>
        public static Type TypeFromExtension(string type)
        {
            return type switch
            {
                "pxad" => typeof(Asset),
                "bmp" => typeof(Bitmap),
                _ => typeof(object),
            };
        }
        public static void ImportFileDialog()
        {
            OpenFileDialog fileDialog = new();
            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                var name = fileDialog.FileName;

                var fileExtension = name.Split('.')[1];
                var typeRef = AssetPipeline.TypeFromExtension(fileExtension);

                if (typeRef != null)
                {
                    var asset = AssetIO.TryDeserializeNonAssetFile(name, typeRef);
                    if (asset == null) return;
                    AssetLibrary.Register(asset.GetType(), asset);
                }
            }
        }
        public static void ImportFileDialog(out Asset? outObject)
        {
            outObject = null; 
            OpenFileDialog fileDialog = new();
            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                var name = fileDialog.FileName;

                var fileExtension = name.Split('.')[1];
                var typeRef = AssetPipeline.TypeFromExtension(fileExtension);

                if (typeRef != null)
                {
                    var asset = AssetIO.TryDeserializeNonAssetFile(name, typeRef);
                    if (asset == null) return;
                    AssetLibrary.Register(asset.GetType(), asset);
                    outObject = asset; 
                }
            }
        }
    }
    /// <summary>
    /// A Runtime static class that allows access to cached, deserialized json objects. 
    /// </summary>
    public static class AssetLibrary
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
            if (LoadedAssets.TryGetValue(typeof(T), out List<Asset> found))
            {
                result = (T)found.Where(x => x.Name.Equals(name));
                return true;
            }
            return false;
        }
        public static bool Fetch<T>(out List<object> output)
        {
            output = new List<object>();  
            foreach (var pair in LoadedAssets)
            {
                var type = pair.Key;
                if (type == typeof(T))
                {
                    foreach (var asset in pair.Value)
                        output.Add(asset);
                    return true;
                }
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
        public static  List<Asset>? Clone()
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
            {
                LoadedAssets.Add(type, new List<Asset>());
            }

            LoadedAssets[type].Add(asset);
        }
        public static void Unregister(Type type, string Name)
        {
            foreach (var asset in LoadedAssets[type])
            {
                if (asset.Name.Equals(Name))
                {
                    LoadedAssets[type].Remove(asset);
                }
            }
        }
    }
}