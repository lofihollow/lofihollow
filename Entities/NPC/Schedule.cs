using Newtonsoft.Json; 
using System.Collections.Generic; 

namespace LofiHollow.Entities.NPC {
    [JsonObject(MemberSerialization.OptIn)]
    public class Schedule {
        [JsonProperty]
        public List<ScheduleNode> Nodes;

        [JsonProperty]
        public int NextNodeTime;

        [JsonProperty]
        public int CurrentNode; 
    }
}
