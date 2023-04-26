using pixel_core.Statics;

namespace pixel_core.FileIO
{
    public abstract class FileBase
    {
        public string extension = "";
        public string fullPath = "C:\\\\Users\\Josh\\Appdata\\Roaming\\Pixel\\Assets\\Metadata\\Error";
        public string Name = "Object Metadata";
        public string pathFromProjectRoot = "";
        internal string _uuid = "";
        public string UUID => _uuid;
        /// <summary>
        /// the absolute path corrected to the machine that's running the program, and WorkingRoot in Constants.
        /// </summary>
        public string Path => Constants.WorkingRoot + pathFromProjectRoot;
    }
}
