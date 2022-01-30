using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames {
    [JsonObject(MemberSerialization.OptOut)]
    public class Egg {
        public MonsterPenMonster HatchesInto;
        public int DaysToHatch = 0;
    }
}
