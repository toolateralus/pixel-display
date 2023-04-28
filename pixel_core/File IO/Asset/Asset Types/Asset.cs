using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.Statics;

namespace Pixel.FileIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Asset
    {
        [JsonProperty] public string Name = "NULL";
        [JsonProperty] public string UUID = "NULL";
        [JsonProperty] public Metadata Metadata;
        public virtual void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.AssetsDir + "\\" + Name + Constants.AssetsExt;
            Metadata = new(defaultPath);
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
