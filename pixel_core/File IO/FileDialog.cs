
using Microsoft.Win32;
using Pixel.FileIO;
using Pixel.Statics;
using System.IO;
using System.Linq;

namespace Pixel.Assets
{
    public class FileDialog
    {
        public static Metadata ImportFileDialog()
        {
            OpenFileDialog fileDialog = new();
            bool? result = fileDialog.ShowDialog();
            Metadata meta = new("temp" + UUID.NewUUID());
            if (result == true)
            {
                var fullPath = Path.GetFullPath(fileDialog.FileName);
                meta = new(fullPath);
            }
            return meta;
        }
    }
}
