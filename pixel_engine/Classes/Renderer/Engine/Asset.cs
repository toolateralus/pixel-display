using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Newtonsoft.Json;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    /// <summary>
    /// base class for all pixel_engine asset references; 
    /// </summary>
    
    public class Asset
    {
        public string pathFromRoot = "";
        public string fileSize = "";
        public string fileType = "";
        public Type runtimeType; 
        public string UUID = "";
        public string Name = "NewAsset";
        public Asset() { }
        public Asset(string name)
        {
            Name = name; 
        }
        public Asset(string pathFromRoot, string fileType, Type runtimeType)
        {
            this.pathFromRoot = pathFromRoot;
            this.fileType = fileType;
            this.runtimeType = runtimeType; 
            UUID = pixel_renderer.UUID.NewUUID();
            AssetLibrary.Register(GetType(), this);
        }
    }
    
    public class BitmapAsset : Asset
    {
        public Bitmap currentValue; 
        public Color[,] colors 
        {
            get  => ColorArrayFromBitmap(currentValue);
        }
      
        public static Bitmap BitmapFromColorArray(Color[,] colors)
        {
            Bitmap bitmap = new(64, 64);  
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
        new public string Name = "NewFontAsset";
        public Dictionary<char, Bitmap> characters = new();
    }

    public static class FontAssetFactory
    {

        public static FontAsset CreateFont(int start, int end, Bitmap[] characters)
        {
            FontAsset fontAsset = new();
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
        /// Really expensive text rendering; 
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static Dictionary<Bitmap, Vec2> ToString(FontAsset asset, string text)
        {
            Dictionary<Bitmap, Vec2> output = new();
            int i = 0;
            foreach (char character in text)
            {
                var x = character;
                if (char.IsLower(character))
                {
                     x = char.ToUpper(character); 
                }
                var img = (Bitmap)asset.characters[x].Clone(); 
                output.Add(img, new Vec2(i + img.Width, 0));
                i++;
            }
            return output; 
        }

        internal static void InitializeDefaultFont()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = appdata + Constants.FontDirectory;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            IEnumerable<string> files = Directory.GetFiles(path);
            int i = 0; 
            foreach (string file in files)
            {
                BitmapAsset bitmap = new()
                {
                    currentValue = new(file),
                    Name = $"{'a' + i}"
                };
                i++;
                AssetLibrary.Register(bitmap.GetType(), bitmap);
            }
        }
    }
    
    /// <summary>
    /// a purpose built json IO class for Asset objects (will be made into a Metadata type object most likely)
    /// </summary>
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
            if (File.Exists(Path + "/" + fileName + ".json"))
            {
                if (!skippingOperation)
                {
                    var overwriteWarningResult = MessageBox.Show($"Are you sure you want to overwrite {fileName}.json ? \n found at {Path}",
                                            "", MessageBoxButton.YesNoCancel, MessageBoxImage.Error,
                                             MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                    var doForAllResult = MessageBox.Show($"Do for all (uses last choice)",
                                            "", MessageBoxButton.YesNo, MessageBoxImage.Error,
                                             MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
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
            TextWriter writer = new StreamWriter(Path + "/" + fileName + ".json");
            var json = JsonSerializer.Create();
            json.Serialize(writer, data);
            writer.Close();
        }
        public static Asset? ReadAssetFile(string fileName)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                return null; 
            }
            
            JsonSerializer serializer = new();
            
            StreamReader reader = new(fileName);
            
            Asset asset = new(); 

            using JsonTextReader json = new(reader);
            try
            {
                asset = serializer.Deserialize<Asset>(json) ?? null;
            }
            catch (Exception _) { throw; };
               
            return asset;
        }
    }

    public static class AssetPipeline
    {
        public static string Path => Constants.WorkingDirectory + Constants.AssetsDirectory;
        /// <summary>
        /// Enumerates through all files in the Asset Import path and attempts to register them to the runtime AssetLibrary instance. 
        /// </summary>
        public static void ImportAsync()
        {
            if (Runtime.Instance.IsRunning)
            {
                var msg = MessageBox.Show("Pixel needs to Import: Press OK to start.", "Asset Importer", MessageBoxButton.OK);
                if (msg == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path); 
            }
            foreach (var dir in Directory.GetDirectories(Path))
            {
                foreach(var file in Directory.GetFiles(dir))
                {
                    var asset = Import(file);
                    if (asset is not null)
                        AssetLibrary.Register(asset.GetType(), asset);
                  
                };
            }
            if (!Runtime.Instance.IsRunning)
            {
                var syncResult = MessageBox.Show("Import complete! Do you want to sync?", "Asset Importer", MessageBoxButton.YesNo);
                if (syncResult == MessageBoxResult.Yes) AssetLibrary.Sync();
            }

        }
        /// <summary>
        /// Read and deserialize a single file from Path"
        /// </summary>
        /// <param name="path"> the file path that will be read from ie. C:\\User\\AppData\\Pixel\\ProjectA\\Asssets\\heanti.gif</param>
        /// <returns></returns>
        public static Asset? Import(string path)
        {
            if (!File.Exists(path)) return null; 
            return AssetIO.ReadAssetFile(path);
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
        public static bool TryFindAsset<T>(string name, out T result) where T : Asset
        {
            result = null; 
            if (LoadedAssets.TryGetValue(typeof(T), out List<Asset> found))
            {
                foreach (var _asset in found)
                {
                    if (_asset is null) continue; 

                    if (_asset.Name.Equals(name))
                    {
                        result = _asset as T; 
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Save the currently loaded asset database.
        /// </summary>
        public static void Sync()
        {
            var library = GetLibrary();
            if (library is null) return; 
            AssetIO.skippingOperation = false; 
            foreach (var asset in library) AssetIO.SaveAsset(asset, asset.Name);
        }
        internal static  List<Asset>? GetLibrary()
        {
            List<Asset> library = new();  
            foreach (var key in LoadedAssets)
                foreach (var item in key.Value)
                    library.Add(item);

            return library; 
        }
        internal static void Register(Type type, Asset asset)
        {
            if (!LoadedAssets.ContainsKey(type))
            {
                LoadedAssets.Add(type, new List<Asset>());
            }
            LoadedAssets[type].Add(asset);
        }
        internal static void Unregister(Type type, string Name)
        {
            foreach (var asset in LoadedAssets[type])
            {
                if (asset.Name.Equals(Name))
                {
                    LoadedAssets[type].Remove(asset);
                }
            }
        }

        internal static bool GetAssetsOfType<T>(out List<T> output)
        {
            output = new();  
            foreach (var type in LoadedAssets)
            {
                if (type.Key.GetType() == typeof(T))
                {
                    output = type.Value as List<T>;
                    return true; 
                }
            }
            return false; 
        }
    }
}