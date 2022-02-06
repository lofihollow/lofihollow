using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using ProtoBuf;

namespace LofiHollow.Minigames.Photo {
    [JsonObject(MemberSerialization.OptOut)]
    [ProtoContract]
    public class PhotoEntity {
        [ProtoMember(1)]
        public string Name = "";
        [ProtoMember(2)]
        public string Type = "";
        [ProtoMember(3)]
        public int X = 0;
        [ProtoMember(4)]
        public int Y = 0;

        [ProtoMember(5)]
        public int Glyph = 32;
        [ProtoMember(6)]
        public int ColR = 0;
        [ProtoMember(7)]
        public int ColG = 0;
        [ProtoMember(8)]
        public int ColB = 0;
        [ProtoMember(9)]
        public int ColA = 0;
        [ProtoMember(10)]
        public Decorator Dec;

        [JsonConstructor]
        public PhotoEntity() { }

        public PhotoEntity(string name, int r, int g, int b, int a, int glyph, Decorator dec) {
            Name = name;
            ColR = r;
            ColG = g;
            ColB = b;
            ColA = a;
            Glyph = glyph;

            if (dec != null) {
                Dec = new();
                Dec.R = dec.R;
                Dec.G = dec.G;
                Dec.B = dec.B;
                Dec.A = dec.A;
                Dec.Glyph = dec.Glyph;
            }
        }

        public ColoredString Appearance() {
            return new ColoredString(((char)Glyph).ToString(), new Color(ColR, ColG, ColB, ColA), Color.Black);
        }

        public CellDecorator GetDec() {
            return new CellDecorator(new Color(Dec.R, Dec.G, Dec.B, Dec.A), Dec.Glyph, Mirror.None);
        }
    }
}
