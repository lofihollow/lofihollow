using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames {
    [JsonObject(MemberSerialization.OptOut)]
    public class MonsterPenFood {
        public string Type = "Meat";
        public int Satiety = 10;
    }
}
