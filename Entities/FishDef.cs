using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using ProtoBuf;

namespace LofiHollow.Entities {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class FishDef {
        [ProtoMember(1)]
        public string PackageID = "";
        [ProtoMember(2)]
        public string Name = "";
        [ProtoMember(3)]
        public string Season = "";
        [ProtoMember(4)]
        public int EarliestTime = 360;
        [ProtoMember(5)]
        public int LatestTime = 360;
        [ProtoMember(6)]
        public string CatchLocation = "";

        [ProtoMember(7)]
        public int MaxQuality = 1;

        [ProtoMember(8)]
        public int RequiredLevel = 0;
        [ProtoMember(9)]
        public int GrantedExp = 0;

        [ProtoMember(10)]
        public int Strength = 0;
        [ProtoMember(11)]
        public int FightChance = 10;
        [ProtoMember(12)]
        public int FightLength = 0;

        [ProtoMember(13)]
        public int colR = 0;
        [ProtoMember(14)]
        public int colG = 0;
        [ProtoMember(15)]
        public int colB = 0;
        [ProtoMember(16)]
        public int colA = 255;
        [ProtoMember(17)]
        public int glyph = 0;

        [ProtoMember(18)]
        public string FishItemID = "";
        [ProtoMember(19)]
        public string FilletID = "";

        public ColoredString GetAppearance() {
            return new ColoredString(((char) glyph).ToString(), new Color(colR, colG, colB), Color.Black);
        }
    }
}
