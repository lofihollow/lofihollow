using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Generic; 

namespace LofiHollow.Entities.NPC {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class Schedule {
        [ProtoMember(1)]
        public List<string> Nodes = new();
        [ProtoMember(2)]
        public int NextNodeTime;
        [ProtoMember(3)]
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
