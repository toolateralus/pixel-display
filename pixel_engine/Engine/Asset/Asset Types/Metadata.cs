using Newtonsoft.Json;

namespace pixel_renderer.IO
{
    public class Metadata : FileBase
    {
        public Metadata(string name, string fullPath, string extension)
        {
            Name = name;
            this.fullPath = fullPath;
            this.extension = extension;
            pathFromProjectRoot = Project.GetPathFromRoot(fullPath);
            _uuid = pixel_renderer.UUID.NewUUID();
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
