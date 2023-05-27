using Newtonsoft.Json;
using Pixel.Statics;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Pixel.FileIO
{
    public class Metadata : FileBase
    {
        public Metadata(string fullPath)
        {
            Name = Path.GetFileNameWithoutExtension(fullPath);
            Extension = Path.GetExtension(fullPath);
            if (Path.GetDirectoryName(fullPath) is string directory)
                RelativeDirectory = Project.GetPathFromRoot(directory);
            _uuid = Statics.UUID.NewUUID();
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
            RelativeDirectory = Constants.AssetsDir;
            Extension = Constants.AssetsExt;
        }
    }
}
