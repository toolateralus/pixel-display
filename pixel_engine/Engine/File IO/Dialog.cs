
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace pixel_renderer.Assets
{
    public class FileDialog
    {
        public Type type;
        public string fileName;
        public string fileExtension;
        public string name;
        public string filePath; 
        public FileDialog(Type type, string name, string filePath, string fileName, string fileExtension)
        {
            this.filePath = filePath;
            this.type = type;
            this.name = name; 
            this.fileName = fileName;
            this.fileExtension = fileExtension;
        }
       
        public static FileDialog ImportFileDialog()
        {
            OpenFileDialog fileDialog = new();
            bool? result = fileDialog.ShowDialog();
            FileDialog dlg = new(null, "", "", "" ,"");
            if (result == true)
            {
                var fullPath = Path.GetFullPath(fileDialog.FileName);
                var name = fileDialog.FileName;
                var split = name.Split('.');
                var extension = split.Last();
                var type = Importer.TypeFromExtension(extension);
                var fileName = split[0].Split("\\").Last();
                dlg = new FileDialog(type, name, fullPath, fileName, extension);
            }
            return dlg;
        }
    }
}
