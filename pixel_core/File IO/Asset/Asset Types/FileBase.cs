using Pixel.Statics;
using System.Text.Json.Serialization;

namespace Pixel.FileIO
{
    public abstract class FileBase
    {
        private string extension = "";
        private string name = "Object Metadata";
        private string relativeDirectory = "";
        internal string _uuid = "";
        [JsonIgnore] private string? fullPath = null;
        public string UUID => _uuid;
        public string Extension
        {
            get => extension;
            set
            {
                extension = value;
                fullPath = null;
            }
        }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                fullPath = null;
            }
        }
        public string RelativeDirectory
        {
            get => relativeDirectory;
            set
            {
                relativeDirectory = value;
                fullPath = null;
            }
        }
        [JsonIgnore] public string FullPath => fullPath ??= Constants.WorkingRoot + RelativeDirectory + "\\" + Name + Extension;
        [JsonIgnore] public string RelativePath => RelativeDirectory + "\\" + Name + Extension;
        [JsonIgnore] public string Directory => Constants.WorkingRoot + RelativeDirectory;
        [JsonIgnore] public string FileName => Name + Extension;
    }
}
