using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

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
        [JsonConstructor]
        public Asset(string name, Type fileType)
        {
            Name = name;
            this.fileType = fileType;
        }
        public Asset() { }

        new public Type GetType() => fileType;

    }
    public class BitmapAsset : Asset
    {
        public Bitmap? RuntimeValue = null;
        public BitmapAsset(string name) : base(name, typeof(Bitmap))
        {
            Name = name;
            fileType = typeof(Bitmap);
        }
        public static BitmapAsset BitmapToAsset(string fileName, string assetName)
        {
            Bitmap? bmp = new(fileName);
            BitmapAsset asset = new(assetName);
            if (bmp != null)
                asset.RuntimeValue = bmp;
            return asset;
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
                // cache here to force uppercase without modifying the asset.
                var _char = character;
                
                if (char.IsLower(character))
                    _char = char.ToUpper(character);

                if (asset.characters.ContainsKey(_char))
                {
                    var img = (Bitmap)asset.characters[_char].Clone();
                    output.Add(img);
                }
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
        public FontAsset(string name, Type fileType) : base(name, typeof(pixel_renderer.FontAsset))
        {
            fileType = typeof(FontAsset);
            Name = name; 
        }
    }
    public class StageAsset : Asset
    {
        public Stage RuntimeValue;
        public StageAsset(string name, Type? fileType, Stage runtimeValue) : base(name, fileType)
        {
            this.fileType = typeof(Stage);
            this.Name = name;
            this.RuntimeValue = runtimeValue;
        }
    }
    public class NodeAsset : Asset
    {
        public Node RuntimeValue;
        public NodeAsset(string name, Node runtimeValue)
        {
            RuntimeValue = runtimeValue;
            Name = name;
            fileType = typeof(Node);
        }
    }
    public class ProjectAsset :  Asset
    {
        public Settings settings; 
        public Runtime runtime;
        public List<StageAsset> stages; 
        public int stageIndex;

        public ProjectAsset(string name) : base(name, typeof(ProjectAsset))
        {
            Name = name;
            fileSize = "";
            settings = new();
            runtime = Runtime.Instance;
            stages = new(); 
            stageIndex = 0; 
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

            using TextWriter writer = new StreamWriter(Path + "/" + fileName + Settings.AssetsFileExtension);

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
                asset = jsonSerializer.Deserialize<Asset>(json);
            }
            catch (Exception) { MessageBox.Show("File read error - Fked Up Big Time"); };
            asset.Name += asset.GetType() ?? null;
            return asset;
        }
        public static void TryDeserializeAssetFIle(ref Asset? outObject, string name)
        {
            Asset? _asset = ReadAssetFile(name);
            if (_asset is null) return;
            outObject = _asset;
            AssetLibrary.Register(_asset.GetType(), _asset);
            return;
        }
        public static Asset? TryDeserializeNonAssetFile(string fileName, Type type, string assetName)
        {
            switch (type)
            {
                case var _ when type == typeof(Bitmap):

                    BitmapAsset bmpAsset = BitmapAsset.BitmapToAsset(fileName, assetName);
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
            var randomIntString = JRandom.Int(0, 250).ToString(); 
            foreach (var asset in from dir in Directory.GetDirectories(Path)
                                  from file in Directory.GetFiles(dir)
                                  let typeString = file.Split('.')[1]
                                  let typeRef = TypeFromExtension(typeString)
                                  let asset = TryPullObject(file, typeRef, typeString + randomIntString)
                                  where asset is not null
                                  select asset)
            {
                asset.fileType ??= typeof(Asset);
                AssetLibrary.Register(asset.fileType, asset);
            };
        }
        /// <summary>
        /// Read and deserialize a single file from Path"
        /// </summary>
        /// <param name="path"> the file path that will be read from ie. C:\\User\\AppData\\Pixel\\ProjectA\\Asssets\\heanti.gif</param>
        /// <returns>Asset if it exists at path, else null.</returns>
        public static Asset? TryPullObject(string path, Type type, string newName)
        {
            return !File.Exists(path) ? null : AssetIO.TryDeserializeNonAssetFile(path, type, newName);
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
                var split = name.Split('.');
                var fileExtension = split[1];
                var typeRef = AssetPipeline.TypeFromExtension(fileExtension);
                var newFileName = split[0].Split("\\").Last(); 
                if (typeRef != null)
                {
                    var asset = AssetIO.TryDeserializeNonAssetFile(name, typeRef, newFileName);
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
            if (result is not true) return;

            string name = fileDialog.FileName;
            var split = name.Split('.');
            string fileExtension = split[1];
            var newFileName = split[0].Split("\\").Last();

            Type typeRef = TypeFromExtension(fileExtension);
            if (typeRef is null) return;
            if (typeRef == typeof(Asset))
            {
                AssetIO.TryDeserializeAssetFIle(ref outObject, name);
                return;
            }
            var asset = AssetIO.TryDeserializeNonAssetFile(name, typeRef, newFileName);
            if (asset is null) return;

            AssetLibrary.Register(asset.GetType(), asset);
            outObject = asset;
        }
    }
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