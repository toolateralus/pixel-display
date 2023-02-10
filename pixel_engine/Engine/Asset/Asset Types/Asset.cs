using Newtonsoft.Json;
using System;
using System.Xml.Linq;

namespace pixel_renderer.FileIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Asset 
    {
        [JsonProperty] public string Name;
        [JsonProperty] public string UUID;
        [JsonProperty] public Metadata Metadata; 

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
