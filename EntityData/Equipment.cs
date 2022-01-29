using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace LofiHollow.EntityData {
    public class Equipment {
        public string DamageType = "";
        public string CombatType = "";
        public int WeaponTier = 0;

        public int ArmorVsSlash = 0;
        public int ArmorVsStab = 0;
        public int ArmorVsCrush = 0;
        public int ArmorVsRange = 0;
        public int ArmorVsMagic = 0;

        public int StabBonus = 0;
        public int SlashBonus = 0;
        public int CrushBonus = 0;
        public int RangeBonus = 0;
        public int MagicBonus = 0;

        public int StrengthBonus = 0;

        [JsonConstructor]
        public Equipment(){

        } 
    }
}
