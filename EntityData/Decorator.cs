using Newtonsoft.Json;
using SadConsole;
using ProtoBuf;

namespace LofiHollow.EntityData {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class Decorator {
        [ProtoMember(1)]
        public int Glyph = 0;
        [ProtoMember(2)]
        public int R = 0;
        [ProtoMember(3)]
        public int G = 0;
        [ProtoMember(4)]
        public int B = 0;
        [ProtoMember(5)]
        public int A = 255;

        [JsonConstructor]
        public Decorator() { }

        public Decorator(Decorator other) {
            Glyph = other.Glyph;
            R = other.R;
            G = other.G;
            B = other.B;
            A = other.A;
        }


        public CellDecorator GetDec() {
            return new CellDecorator(new SadRogue.Primitives.Color(R, G, B, A), Glyph, Mirror.None);
        }
    }
}
