using Newtonsoft.Json;
using Pixel.Statics;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Pixel.FileIO
{
    public class Metadata : FileBase
    {

        public Metadata(string fullPath)
        {
            string name_and_extension = "";
            string[] backslash_split = fullPath.Split('\\');

            foreach (string str in backslash_split)
                if (str.Contains('.'))
                    name_and_extension = str;

            string name = "";
            string extension = ""; 
            string[] period_split = name_and_extension.Split('.');

            if (period_split.Any())
            {
                name = period_split.First();
                extension = period_split.Last();
            }

            Interop.Log("Name and Extension : " + name_and_extension);
            Interop.Log("FullPath : " + fullPath);

            Name = name;
            this.fullPath = fullPath;
            _uuid = Statics.UUID.NewUUID();
            this.extension = VerifyExtension(extension);
            pathFromProjectRoot = Project.GetPathFromRoot(fullPath);
        }
        private static string VerifyExtension(string extension)
        {
            int periods = 0;

            foreach (var c in extension)
                if (c == '.')
                    periods++;

            if (periods != 1)
            {
                if (periods > 0)
                    extension = extension.Replace(".", "");
                extension = "." + extension;
            }

            return extension;
        }
        public Metadata()
        {
            Name = "Default Metadata";
            fullPath = Constants.WorkingRoot + Constants.AssetsDir + Name + Constants.AssetsExt;
        }
        [JsonConstructor]
        public Metadata(string name, string fullPath, string pathFromProjectRoot, string uuid, string extension)
        {
            Name = name;
            this.fullPath = fullPath;
            this.extension = extension;
            this.pathFromProjectRoot = pathFromProjectRoot;
            _uuid = uuid;
        }
    }
}
