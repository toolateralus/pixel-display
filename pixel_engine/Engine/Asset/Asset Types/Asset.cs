using System;

namespace pixel_renderer.FileIO
{
    public class Asset : FileBase
    {
        public Asset(string Name, string? UUID = null)
        {
            this.fullPath = Constants.AssetsDir + "\\" + Name;
            this.Name = Name;
            _uuid = UUID ?? pixel_renderer.UUID.NewUUID(); 
        }
        public Asset() 
        {
        }
    }
}
