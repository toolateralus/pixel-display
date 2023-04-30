using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.Statics;

namespace Pixel.FileIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Asset
    {
        static int instance_ct;
        [JsonProperty] public string Name = "Null Asset!";
        [JsonProperty] public string UUID = "ae86-1999-krack-uuid";
        [JsonProperty] public Metadata Metadata;
        public virtual void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.AssetsDir + "\\" + Name + Constants.AssetsExt;
            Metadata = new(defaultPath);
        }
        internal protected void Save()
        {
            IO.GuaranteeUniqueName(Metadata, this);
            IO.WriteJson(this, Metadata);
        }
        public Asset(string name, bool shouldUpload = false)
        {
            Name = name;
            Name += instance_ct++;
            UUID = Pixel.Statics.UUID.NewUUID();
            if (shouldUpload) Upload();
        }

        public void Upload()
        {
            Sync();
            Library.Register(ref Metadata, this);
        }
    }
}
