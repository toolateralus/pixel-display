using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pixel_renderer.FileIO
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
            _uuid = pixel_renderer.UUID.NewUUID();
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
