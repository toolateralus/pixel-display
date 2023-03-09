using Newtonsoft.Json;
using pixel_renderer.Assets;
using System;
using System.CodeDom;
using System.DirectoryServices.ActiveDirectory;
using System.Xml.Linq;

namespace pixel_renderer.FileIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Asset 
    {
        [JsonProperty] public string Name;
        [JsonProperty] public string UUID;
        [JsonProperty] public Metadata Metadata;
        public Asset() : this("New Asset Mode", false)
        {
         
        }

        public virtual void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.AssetsDir + "\\" + Name + Constants.AssetsFileExtension;
            Metadata = new(Name, defaultPath, Constants.AssetsFileExtension);
        }
    
        public Asset(string name = "New Asset", bool shouldUpload = false)
        {
            Name = name;
            UUID = pixel_renderer.UUID.NewUUID();
            if(shouldUpload) Upload();
        }

        public void Upload()
        {
            Sync();
            AssetLibrary.Register(Metadata, this);
        }
    }
}
