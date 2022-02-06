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
    public class PhotoTile {
        public string Name;
        public int ColR;
        public int ColG;
        public int ColB;
        public int ColA;
        public int Glyph;
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
