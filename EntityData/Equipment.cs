using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptOut)]
    public class Equipment {
        public string DamageType = ""; 
        public int WeaponTier = 0;

        public int Armor = 0;
        public int MagicArmor = 0; 
    }
}
