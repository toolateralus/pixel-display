using Newtonsoft.Json;
using pixel_renderer.Assets;
using System;
using System.CodeDom;
using System.DirectoryServices.ActiveDirectory;
using System.Xml.Linq;

namespace pixel_renderer.FileIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Asset 
    {
        [JsonProperty] public string Name;
        [JsonProperty] public string UUID;
        [JsonProperty] public Metadata Metadata;

        public virtual bool Sync()
        {
            try
            {
                string defaultPath = Constants.WorkingRoot + Constants.AssetsDir + "\\" + Name + Constants.AssetsFileExtension;
                Metadata = new(Name, defaultPath, Constants.AssetsFileExtension);
                return true;
            }
            catch { return false; }
               
        }
        public Asset(string Name, string UUID) : this()
        {
            this.Name = Name;
            this.UUID = UUID;
        }

        public Asset(bool shouldUpload = false)
        {
            Name = "New Asset";
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
