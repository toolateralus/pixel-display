
using Microsoft.Win32;
using pixel_core.FileIO;
using System.IO;
using System.Linq;

namespace pixel_core.Assets
{
    public class FileDialog
    {
        public static Metadata ImportFileDialog()
        {
            OpenFileDialog fileDialog = new();
            bool? result = fileDialog.ShowDialog();
            Metadata meta = new("", "", "");
            if (result == true)
            {
                var fullPath = Path.GetFullPath(fileDialog.FileName);
                var name = fileDialog.FileName;
                var split = name.Split('.');
                var extension = split.Last();
                var fileName = split[0].Split("\\").Last();
                meta = new(fileName, fullPath, extension);
            }
            return meta;
        }
    }
}
