using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.Statics;

namespace Pixel.FileIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Asset
    {
        [JsonProperty] public string Name;
        [JsonProperty] public string UUID;
        [JsonProperty] public Metadata Metadata;
        public Asset()
        {
        }
        public virtual void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.AssetsDir + "\\" + Name + Constants.AssetsFileExtension;
            Metadata = new(Name, defaultPath, Constants.AssetsFileExtension);
        }
        internal protected void Save()
        {
            AssetIO.GuaranteeUniqueName(Metadata, this);
            IO.WriteJson(this, Metadata);
        }
        public Asset(string name, bool shouldUpload = false)
        {
            Name = name;
            UUID = Pixel.Statics.UUID.NewUUID();
            if (shouldUpload) Upload();
        }

        public void Upload()
        {
            Sync();
            AssetLibrary.Register(Metadata, this);
        }
    }
}
