
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using pixel_renderer.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace pixel_renderer.Assets
{
    public class Importer
    {
        public static string Path => Constants.WorkingRoot + Constants.AssetsDir;
        /// <summary>
        /// Enumerates through all files in the Asset Import path and attempts to register them to the runtime AssetLibrary instance. 
        /// </summary>
        public static void Import(bool showMessage = false)
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

               ImportTask(); 

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
            var subdirectories = Directory.EnumerateDirectories(Path); 
            foreach (var file in Directory.EnumerateFiles(Path))
                FinalImport(file);

            if (!subdirectories.Any())
                     return;

            foreach (var dir in subdirectories)
                foreach (var file in Directory.EnumerateFiles(dir))
                    FinalImport(file);
        }

        private static void FinalImport(string file)
        {
            var fileExtension = file.Split('.')[1];
            var typeRef = TypeFromExtension(fileExtension);
            var asset = TryPullObject(file, typeRef, fileExtension);

            if (asset is not null)
            {
                Metadata meta = new(asset.Name, file, fileExtension);
                asset.fileType ??= typeof(Asset);
                Library.Register(asset.fileType, asset);
                Library.RegisterMetadata(meta, asset);
            };
        }

        /// <summary>
        /// Read and deserialize a single file from Path"
        /// </summary>
        /// <param name="path"> the file path that will be read from ie. C:\\User\\AppData\\Pixel\\ProjectA\\Asssets\\heanti.gif</param>
        /// <returns>Asset if it exists at path, else null.</returns>
        public static Asset? TryPullObject(string path, Type type, string newName)
        {
            return !File.Exists(path) ? null : AssetIO.TryDeserializeNonAssetFile(newName, type, path);
        }
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns>type if supported, else returns typeof(object) (generic) </returns>
        public static Type TypeFromExtension(string type)
        {
            if (type[0] == '.')
                type = type.Remove('.');

            return type switch
            {
                "pxad" => typeof(Asset),
                "pxpj" => typeof(Project),
                "bmp" => typeof(Bitmap),
                 _ => typeof(object),
            };
        }
        public static string ExtensionFromType(Type type)
        {
            if (type == typeof(Asset))
                return ".pxad";
            if (type == typeof(Project))
                return ".pxpj";
            if (type == typeof(Bitmap))
                return ".bmp";
            return "File Extension not found from type, either it's unsupported or was edited";
        }
        public static void ImportAssetDialog()
        {
            FileDialog dialog = FileDialog.ImportFileDialog();
            if (dialog.type != null)
            {
                var asset = AssetIO.TryDeserializeNonAssetFile(dialog.fileName, dialog.type, dialog.filePath);
                if (asset == null) return;
                Library.Register(asset.GetType(), asset);
            }
        }
    }
}
