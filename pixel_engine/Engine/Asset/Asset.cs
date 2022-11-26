
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
using pixel_renderer;
using pixel_renderer.IO; 
using pixel_renderer.Assets;
using System.Runtime.CompilerServices;

namespace pixel_renderer.Assets
{
    [JsonObject]
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
    public class Dialog
    {
        public Type type;
        public string fileName;
        public string fileExtension;
        public string name; 
        public Dialog(Type type, string name, string fileName, string fileExtension)
        {
            this.type = type;
            this.name = name; 
            this.fileName = fileName;
            this.fileExtension = fileExtension;
        }
        public Dialog()
        {
            type = null;
            name = "";
            fileName = "";
            fileExtension = "";
        }
        public static Dialog ImportFileDialog()
        {
            OpenFileDialog fileDialog = new();
            bool? result = fileDialog.ShowDialog();
            Dialog dlg = new();
            if (result == true)
            {
                var name = fileDialog.FileName;
                var split = name.Split('.');
                var ext = split.Last();
                var type = Importer.TypeFromExtension(ext);
                var fileName = split[0].Split("\\").Last();
                dlg = new(type, name, fileName, ext);
            }
            return dlg;
        }
    }
    public class Importer
    {
        public static string Path => Settings.AppDataDir + Settings.AssetsDir;
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

                    if (syncResult == MessageBoxResult.Yes) Library.Sync();
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
                Library.Register(asset.fileType, asset);
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
                "pxpj" => typeof(Project),
                "bmp" => typeof(Bitmap),
                 _ => typeof(object),
            };
        }
        public static void ImportAssetDialog()
        {
            Dialog dialog = Dialog.ImportFileDialog();
            if (dialog.type != null)
            {
                var asset = AssetIO.TryDeserializeNonAssetFile(dialog.name, dialog.type, dialog.fileName);
                if (asset == null) return;
                Library.Register(asset.GetType(), asset);
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

            Library.Register(asset.GetType(), asset);
            outObject = asset;
        }
    }
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
namespace pixel_renderer.IO
{
    public class ProjectIO
    {
        public static string Path => Settings.AppDataDir + Settings.ProjectsDir;
        public static void SaveProject(Project project)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            if (File.Exists(Path + "/" + project.Name + Settings.ProjectFileExtension))
            {
                    var overwriteWarningResult = MessageBox.Show($"Are you sure you want to overwrite Project {project.Name}.pxpj ? \n found at {Path}",
                                            "", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning,
                                             MessageBoxResult.No, MessageBoxOptions.RtlReading);
                    if (overwriteWarningResult != MessageBoxResult.Yes)
                    {
                        return;
                    }
            }
            using TextWriter writer = new StreamWriter(Path + "/" + project.Name + Settings.ProjectFileExtension);
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            var jsonSerializer = JsonSerializer.Create(settings);
            jsonSerializer.Serialize(writer, project);
            writer.Close();
        }
        public static Project ReadProjectFile(string fileName)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                throw new Exception($"Path not found, A Directory was created at {Path}. please try placing data within this directory and try the operation again.");
            }
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            var jsonSerializer = JsonSerializer.Create(settings);
            StreamReader reader = new(Path + "\\" + fileName + Settings.ProjectFileExtension);
            Project  project = new(fileName);
            using JsonTextReader json = new(reader);
                project = jsonSerializer.Deserialize<Project>(json);

            if (project is null) 
                throw new NullReferenceException(); 
            return project;
        }
        public static void TryFetchProject(out Project? outObject, string name)
        {
            outObject = new("Null"); 
            Project project = ReadProjectFile(name);
            if (project is null) return;
            outObject = project;
            return;
        }
    }
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
            return asset;
        }

        public static void TryDeserializeAssetFIle(ref Asset? outObject, string name)
        {
            Asset? _asset = ReadAssetFile(name);
            if (_asset is null) return;
            outObject = _asset;
            Library.Register(_asset.GetType(), _asset);
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

}
    public class Project
    {
        public static Project LoadProject()
        {
            Project project = new("Default");
            Dialog dlg = Dialog.ImportFileDialog();

            if (dlg.type is null)
                return project;

            if (dlg.type.Equals(typeof(Project)))
            {
                project = ProjectIO.ReadProjectFile(dlg.fileName);

                if (project is not null)
                    return project;
                    else return new("Default"); 
            }
            return project;
        }
        public List<StageAsset> stages;
        public Library library;
        public int stageIndex;
        public int fileSize = 0;

        public Project(string name)
        {
            Name = name;
            var stage = Staging.Default();
            StageAsset asset = new("", stage);

            stageIndex = 0;
            stages = new()
            {
                asset
            };

            stageIndex = 0;
            fileSize = 10;
        }

        public string Name { get; }

    }
