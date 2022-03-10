using Newtonsoft.Json;
using Steamworks.Data;

namespace LofiHollow.DataTypes { 
    [JsonObject(MemberSerialization.OptOut)]
    public class ModMetadata { 
        public string WorkshopTitle = "";
        public string WorkshopDesc = "";
        public string Package = "";

        public PublishedFileId PublishedID;
    }
}
