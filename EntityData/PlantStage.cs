using LofiHollow.EntityData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class PlantStage {
        [JsonProperty]
        public int DaysToNext = 1;
        [JsonProperty]
        public string HarvestItem = "";
        [JsonProperty]
        public int ColorR = 0;
        [JsonProperty]
        public int ColorG = 0;
        [JsonProperty]
        public int ColorB = 0;
        [JsonProperty]
        public int Glyph = 0;

        [JsonProperty]
        public Decorator Dec;
    }
}
