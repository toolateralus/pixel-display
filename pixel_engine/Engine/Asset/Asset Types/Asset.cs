using System;
using System.Xml.Linq;

namespace pixel_renderer.FileIO
{
    public class Asset 
    {
        public string Name;
        public string UUID; 
        public Asset(string Name, string UUID)
        {
            this.Name = Name;
            this.UUID = UUID;
        }
        public Asset() 
        {
            Name = "New Asset";
            UUID = pixel_renderer.UUID.NewUUID();
        }
    }
}
