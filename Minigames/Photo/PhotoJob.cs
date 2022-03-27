using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadConsole; 

namespace LofiHollow.Minigames.Photo { 
    [JsonObject(MemberSerialization.OptOut)]
    public class PhotoJob {
        public ColoredString Appearance;
        public Decorator Dec;
        public string Target = "";
        public string Type = "";
        public int RewardCoppers = 0;
        public int Level = 5;

        public PhotoJob() { }

        public PhotoJob(ColoredString app, Decorator dec, string name, string type, int reward) {
            Appearance = app;
            Dec = dec;
            Target = name;
            Type = type;
            RewardCoppers = reward;
        }
    }
}
