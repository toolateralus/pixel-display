using Newtonsoft.Json;

namespace pixel_renderer.IO
{
    public class Metadata<T>
    {
        public string Name = "Object Metadata";
        public string fullPath = "C:\\\\Users\\Josh\\Appdata\\Roaming\\Pixel\\Assets\\Metadata\\Error";
        public string extension = ""; 
        public string pathFromProjectRoot = "";
        private string _uuid = "";
        public string UUID => _uuid;

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
