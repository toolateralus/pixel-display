namespace pixel_renderer.FileIO
{
    public abstract class FileBase
    {
        public string extension = "";
        public string fullPath = "C:\\\\Users\\Josh\\Appdata\\Roaming\\Pixel\\Assets\\Metadata\\Error";
        public string Name = "Object Metadata";
        public string pathFromProjectRoot = "";
        internal string _uuid = "";
        public string UUID => _uuid;
    }
}
