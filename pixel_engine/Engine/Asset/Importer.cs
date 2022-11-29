
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using pixel_renderer.IO;

namespace pixel_renderer.Assets
{
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
}
