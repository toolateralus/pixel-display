using System;
using System.Runtime.CompilerServices;

namespace pixel_renderer.Assets
{
    public record Asset
    {
        public string pathFromRoot = "";
        public string fileSize = "";
        private string _uuid = "";
        public string UUID { get { if (_uuid is null || _uuid == "") _uuid = pixel_renderer.UUID.NewUUID();  return _uuid; }}
        public string Name = "New Asset";
        public Type fileType;
        public Asset(string name, Type fileType, string? UUID = null)
        {
            Name = name;
            this.fileType = fileType;
            _uuid = UUID ?? pixel_renderer.UUID.NewUUID(); 
        }
        public Asset() { }
        new public Type GetType() => fileType;
    }
}
