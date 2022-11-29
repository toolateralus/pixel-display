
using Microsoft.Win32;
using System;
using System.Linq;

namespace pixel_renderer.Assets
{
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
}
