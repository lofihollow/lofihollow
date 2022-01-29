using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)] 
    public class Heal {
        [JsonProperty]
        public string HealAmount = "1d1";
    }
}
