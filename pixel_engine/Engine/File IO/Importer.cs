using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using pixel_renderer.FileIO;
namespace pixel_renderer.Assets
{
    public class Importer
    {
        public static string DirectoryPath => Constants.WorkingRoot + Constants.AssetsDir;
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


            AssetIO.FindOrCreateAssetsDirectory();
            ImportTask(); 

            if (!Runtime.Instance.IsRunning)
                if (showMessage)
                {
                    var syncResult = MessageBox
                        .Show("Import complete! Do you want to sync?",
                        "Asset Importer",
                        MessageBoxButton.YesNo);

                    if (syncResult == MessageBoxResult.Yes) 
                        AssetLibrary.Sync();
                }
        }
        private static void ImportTask()
        {
            foreach (var file in Directory.EnumerateFiles(DirectoryPath))
                FinalImport(file);

            var subdirectories = Directory.EnumerateDirectories(DirectoryPath); 
            if (!subdirectories.Any())
                     return;

            foreach (var dir in subdirectories)
                foreach (var file in Directory.EnumerateFiles(dir))
                    FinalImport(file);
        }

        private static void FinalImport(string file)
        {
            var fileExtension = file.Split('.')[1];
            Metadata meta = new("New Asset", file, fileExtension);
            var asset = TryPullObject(meta);
            if (asset is not null)
            {
                asset.fileType ??= typeof(Asset);
                AssetLibrary.Register((meta, asset));
            };
        }

        /// <summary>
        /// try to read from path specified in metadata and convert to asset type.
        /// </summary>
        /// <param name="path"> the file path that will be read from ie. C:\\User\\AppData\\Pixel\\ProjectA\\Asssets\\heanti.gif</param>
        /// <returns>Asset if it exists at path, else null.</returns>
        public static Asset? TryPullObject(Metadata meta)
        {
            return !File.Exists(meta.fullPath) ? null : IO.ReadJson<Asset>(meta);
        }
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns>type if supported, else returns typeof(object) (generic) </returns>
        public static Type TypeFromExtension(string type)
        {
            if (type.Contains('.'))
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
            Metadata metadata = FileDialog.ImportFileDialog();

            var isPathValid = System.IO.Path.IsPathFullyQualified(metadata.fullPath);
            
            if (!isPathValid) 
                return;

            var asset = IO.ReadJson<Asset>(metadata);
            if (asset is null) return;
            AssetLibrary.Register((metadata, asset));
        }
    }
}
