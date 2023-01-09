using System;

namespace pixel_renderer.IO
{
    public class Asset
    {
        public string Name = "New Asset";
        public string pathFromRoot = "";
        public string filePath = "C:\\Users\\";

        public string fileSize = "";
        public string UUID => _uuid;
        private string _uuid = "";

        public Type fileType;

        new public Type GetType() => fileType;
        
        public Asset(string Name, Type fileType, string? UUID = null)
        {
            this.filePath = Constants.AssetsDir + "\\" + Name;
            this.pathFromRoot = Project.GetPathFromRoot(filePath);
            this.fileType = fileType;
            this.Name = Name;
            _uuid = UUID ?? pixel_renderer.UUID.NewUUID(); 
        }
        public Asset() 
        {
        }
    }
}
