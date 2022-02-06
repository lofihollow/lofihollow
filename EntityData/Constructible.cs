using ProtoBuf;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives; 
using System.Collections.Generic; 

namespace LofiHollow.EntityData {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class Constructible {
        [ProtoMember(1)]
        public string Name = "";
        [ProtoMember(2)]
        public string Materials = "";

        [ProtoMember(3)]
        public int Glyph = 32;

        [ProtoMember(4)]
        public int ForegroundR = 0;
        [ProtoMember(5)]
        public int ForegroundG = 0;
        [ProtoMember(6)]
        public int ForegroundB = 0;

        [ProtoMember(7)]
        public Decorator Dec;

        [ProtoMember(8)]
        public LockOwner Lock;

        [ProtoMember(9)]
        public string SpecialProps = "";

        [ProtoMember(10)]
        public int RequiredLevel = 0;
        [ProtoMember(11)]
        public int ExpGranted = 0;

        [ProtoMember(12)]
        public bool BlocksMove = false;

        [ProtoMember(13)]
        public bool BlocksLOS = false;

        [ProtoMember(14)]
        public Container Container;

        [ProtoMember(15)]
        public List<ConstructionMaterial> MaterialsNeeded = new();


        public ColoredString Appearance() {
            ColoredString output = new(((char)Glyph).ToString(), new Color(ForegroundR, ForegroundG, ForegroundB), Color.Black);

            if (Dec != null) {
                CellDecorator[] dec = new CellDecorator[1];
                dec[0] = new CellDecorator(new Color(Dec.R, Dec.G, Dec.B, Dec.A), Dec.Glyph, Mirror.None);
                output.SetDecorators(dec);
            }

            return output;
        }
    }
}
