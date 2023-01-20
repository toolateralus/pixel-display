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
        private static void FinalImport(string fullPath)
        {
            GetFileNameAndExtensionFromPath(fullPath, out string name, out string ext);

            Metadata meta = new(name, fullPath, ext);
            Asset? asset = TryPullObject(meta) as Asset;
            
            if (asset is not null)
                AssetLibrary.Register((meta, asset));
        }
        public static void GetFileNameAndExtensionFromPath(string path, out string name, out string ext)
        {
            var split = path.Split('\\');
            string nameAndExt = split[^1];
            split = nameAndExt.Split('.');
            name = split[0];
            ext = split[1];
        }
        /// <summary>
        /// try to read from path specified in metadata and convert to asset type.
        /// </summary>
        /// <param name="path"> the file path that will be read from ie. C:\\User\\AppData\\Pixel\\ProjectA\\Asssets\\heanti.gif</param>
        /// <returns>Asset if it exists at path, else null.</returns>
        public static object? TryPullObject(Metadata meta)
        {
            if(meta.extension == Constants.BitmapFileExtension)
                return !File.Exists(meta.fullPath) ? null : meta;
            return null;
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
                Constants.AssetsFileExtension => typeof(Asset),
                Constants.ProjectFileExtension => typeof(Project),
                Constants.MetadataFileExtension => typeof(Metadata),
                Constants.BitmapFileExtension => typeof(Bitmap),
                 _ => typeof(object),
            };
        }
        public static string ExtensionFromType(Type type)
        {
            if (type == typeof(Asset))
                return Constants.AssetsFileExtension;
            if (type == typeof(Project))
                return Constants.ProjectFileExtension;
            if (type == typeof(Bitmap))
                return Constants.BitmapFileExtension;
            if (type == typeof(Metadata))
                return Constants.MetadataFileExtension;

            return "File Extension not found from type, either it's unsupported or was edited";
        }
        public static void ImportAssetDialog()
        {
            Metadata metadata = FileDialog.ImportFileDialog();

            var isPathValid = System.IO.Path.IsPathFullyQualified(metadata.fullPath);
            
            if (!isPathValid) 
                return;
        }
    }
}
