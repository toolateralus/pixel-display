using Newtonsoft.Json;
using pixel_core.Statics;

namespace pixel_core.FileIO
{
    public class Metadata : FileBase
    {
        public Metadata(string name, string fullPath, string extension)
        {
            Name = name;
            this.fullPath = fullPath;
            extension = VerifyExtension(extension);
            this.extension = extension;
            pathFromProjectRoot = Project.GetPathFromRoot(fullPath);
            _uuid = pixel_core.Statics.UUID.NewUUID();
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
            fullPath = Constants.WorkingRoot + Constants.AssetsDir + Name + Constants.AssetsFileExtension;
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
