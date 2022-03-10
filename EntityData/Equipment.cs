using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptOut)]
    public class Equipment {
        public string DamageType = "";
        public int Power = 0;
        public int Accuracy = 0;
        public int Armor = 0;
        public int MagicArmor = 0; 
    }
}
