using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.Statics;
using System.Dynamic;
using System.IO;

namespace Pixel.FileIO
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Asset
    {
        [JsonProperty] public string Name { get => metadata.Name; set => metadata.Name = value; }
        [JsonProperty] public string UUID = "nothing";
        [JsonProperty] public  Metadata metadata = new($"{Constants.WorkingRoot}{Constants.AssetsDir}/NewAsset{Constants.AssetsExt}");
        internal protected void Save()
        {
            IO.MakeFileNameUnique(this);
            Upload();
            IO.WriteJson(this, metadata);
        }
        public Asset(string name, bool shouldUpload = false)
        {
            Name = name;
            UUID = Pixel.Statics.UUID.NewUUID();
            if (shouldUpload) Upload();
        }

        public void Upload()
        {
            Library.Register(metadata, this);
        }
    }
}
