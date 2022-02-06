using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Photo {
    [JsonObject(MemberSerialization.OptOut)]
    public class PhotoEntity {
        public string Name = "";
        public string Type = "";
        public int X = 0;
        public int Y = 0;

        public int Glyph = 32;
        public int ColR = 0;
        public int ColG = 0;
        public int ColB = 0;
        public int ColA = 0;

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
