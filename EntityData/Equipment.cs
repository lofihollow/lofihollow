using System;
using SadRogue.Primitives;
using SadConsole;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptOut)]
    public class Equipment {
        public string DamageType = "";
        public int Power = 0;
        public int Accuracy = 0;
        public int Armor = 0;
        public int MagicArmor = 0;

        public int WornGlyph = 0;
        public int WornR = 0;
        public int WornG = 0;
        public int WornB = 0;


        public CellDecorator[] GetDec() {
            CellDecorator[] dec = new CellDecorator[1];
            dec[0] = new CellDecorator(new Color(WornR, WornG, WornB), WornGlyph, Mirror.None);

            return dec;
        }
    }
}
