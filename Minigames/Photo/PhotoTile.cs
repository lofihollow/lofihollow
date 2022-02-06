using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using ProtoBuf;

namespace LofiHollow.Minigames.Photo {
    [JsonObject(MemberSerialization.OptOut)]
    [ProtoContract]
    public class PhotoTile {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public int ColR;
        [ProtoMember(3)]
        public int ColG;
        [ProtoMember(4)]
        public int ColB;
        [ProtoMember(5)]
        public int ColA;
        [ProtoMember(6)]
        public int Glyph;
        [ProtoMember(7)]
        public Decorator Dec;

        [JsonConstructor]
        public PhotoTile() { }


        public PhotoTile(string name, int r, int g, int b, int a, int glyph, Decorator dec) {
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

        public ColoredString GetAppearance() {
            return new ColoredString(((char)Glyph).ToString(), new Color(ColR, ColG, ColB, ColA), Color.Black);
        }

        public CellDecorator GetDec() {
            return new CellDecorator(new Color(Dec.R, Dec.G, Dec.B, Dec.A), Dec.Glyph, Mirror.None);
        }
    }
}
