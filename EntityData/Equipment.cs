using ProtoBuf;
using Newtonsoft.Json; 

namespace LofiHollow.EntityData {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class Equipment {
        [ProtoMember(1)]
        public string DamageType = "";
        [ProtoMember(2)]
        public string CombatType = "";
        [ProtoMember(3)]
        public int WeaponTier = 0;
        [ProtoMember(4)]
        public int ArmorVsSlash = 0;
        [ProtoMember(5)]
        public int ArmorVsStab = 0;
        [ProtoMember(6)]
        public int ArmorVsCrush = 0;
        [ProtoMember(7)]
        public int ArmorVsRange = 0;
        [ProtoMember(8)]
        public int ArmorVsMagic = 0;
        [ProtoMember(9)]
        public int StabBonus = 0;
        [ProtoMember(10)]
        public int SlashBonus = 0;
        [ProtoMember(11)]
        public int CrushBonus = 0;
        [ProtoMember(12)]
        public int RangeBonus = 0;
        [ProtoMember(13)]
        public int MagicBonus = 0;
        [ProtoMember(14)]
        public int StrengthBonus = 0;
        [ProtoMember(15)]
        public int Reach = 1;
    }
}
