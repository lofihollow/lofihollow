using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class Decorator {
        [JsonProperty]
        public int Glyph = 0;
        [JsonProperty]
        public int R = 0;
        [JsonProperty]
        public int G = 0;
        [JsonProperty]
        public int B = 0;
        [JsonProperty]
        public int A = 255;
    }
}
