using Newtonsoft.Json;
using SadRogue.Primitives;
using System.Collections.Generic;

namespace LofiHollow.Entities.NPC {
    [JsonObject(MemberSerialization.OptIn)]
    public class ScheduleNode {
        [JsonProperty]
        public int DayMinutes; // The minutes into the day that the node should take priority. Range of 0 to 1440. Times after 1440 take place after midnight before the clock rolls over.

        [JsonProperty]
        public Point3D MapPos; // The map tile location the node specifies

        [JsonProperty]
        public Point Position; // The position on the map tile they need to reach



    }
}