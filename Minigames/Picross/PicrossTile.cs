using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Picross {
    public class PicrossTile {
        public bool PartOfSolution = false;
        public int R = 0;
        public int G = 0;
        public int B = 0;
        public bool Crossed = false;
        public bool Checked = false;

        [JsonConstructor]
        public PicrossTile() { }

        public PicrossTile(PicrossTile other) {
            PartOfSolution = other.PartOfSolution;
            R = other.R;
            G = other.G;
            B = other.B;
            Crossed = other.Crossed;
            Checked = other.Checked;
        }

        public ColoredString GetAppearance(bool solved) {
            if (solved)
                return new ColoredString(254.AsString(), new Color(R, G, B), Color.Black);
            if (Crossed)
                return new ColoredString("X", Color.White, Color.Black);
            if (Checked)
                return new ColoredString(254.AsString(), Color.White, Color.Black);
            return new ColoredString(254.AsString(), Color.DarkSlateGray, Color.Black);
        }
    }
}
