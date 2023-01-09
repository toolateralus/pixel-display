using System;

namespace pixel_renderer.IO
{
    public class Asset
    {
        public string Name = "New Asset";
        public string pathFromRoot = "";
        public string filePath = "C:\\Users\\";

        public string fileSize = "";
        private string _uuid = "";
        public string UUID { get { if (_uuid is null || _uuid == "") _uuid = pixel_renderer.UUID.NewUUID();  return _uuid; }}
        public Type fileType;

        new public Type GetType() => fileType;
        
        public Asset(string Name, Type fileType, string? UUID = null)
        {
            this.filePath = Constants.AssetsDir + "\\" + Name;
            this.pathFromRoot = Project.GetPathFromRoot(filePath);
            this.fileType = fileType;
            this.Name = Name;
            this._uuid = UUID ?? pixel_renderer.UUID.NewUUID(); 
        }
        public Asset() 
        {
        }
    }
}
