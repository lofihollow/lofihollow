using Newtonsoft.Json; 
using System.Collections.Generic; 

namespace LofiHollow.Entities.NPC {
    [JsonObject(MemberSerialization.OptIn)]
    public class Schedule {
        [JsonProperty]
        public List<string> Nodes = new();

        [JsonProperty]
        public int NextNodeTime;

        [JsonProperty]
        public int CurrentNode;

        [JsonConstructor]
        public Schedule() { }

        public Schedule(Schedule other) {
            Nodes.Clear();
            for (int i = 0; i < other.Nodes.Count; i++) {
                Nodes.Add(other.Nodes[i]);
            }

            NextNodeTime = other.NextNodeTime;
            CurrentNode = other.CurrentNode;
        }
    }
}
