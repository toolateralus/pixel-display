using System;

namespace pixel_renderer.FileIO
{
    public class Asset 
    {
        public string Name;
        public string UUID; 
        public Asset(string Name, string? UUID = null)
        {
            this.Name = Name;
            this.UUID = UUID;
        }
        public Asset() 
        {
        }
    }
}
