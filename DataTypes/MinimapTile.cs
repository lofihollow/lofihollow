using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;

namespace LofiHollow.DataTypes {
    public class MinimapTile {
        public string name = "Void";
        public char ch = ' ';
        public Color fg = Color.White;
        public Color bg = Color.Black;

        [JsonConstructor]
        public MinimapTile(char inCh, Color inFG, Color inBG) {
            ch = inCh;
            fg = inFG;
            bg = inBG;
        }

        public MinimapTile(MinimapTile other) {
            name = other.name;
            ch = other.ch;
            fg = other.fg;
            bg = other.bg;
        }

        public ColoredString AsColoredGlyph() {
            return new ColoredString(ch.ToString(), fg, bg); 
        }

    }
}
