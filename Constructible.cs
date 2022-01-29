using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow {
    [JsonObject(MemberSerialization.OptIn)]
    public class Constructible {
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public string Materials = "";

        [JsonProperty]
        public int Glyph = 32;

        [JsonProperty]
        public int ForegroundR = 0;
        [JsonProperty]
        public int ForegroundG = 0;
        [JsonProperty]
        public int ForegroundB = 0;

        [JsonProperty]
        public Decorator Dec;

        [JsonProperty]
        public LockOwner Lock;

        [JsonProperty]
        public string SpecialProps = "";

        [JsonProperty]
        public int RequiredLevel = 0;
        [JsonProperty]
        public int ExpGranted = 0;

        [JsonProperty]
        public bool BlocksMove = false;

        [JsonProperty]
        public bool BlocksLOS = false;

        [JsonProperty]
        public Container Container;

        [JsonProperty]
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
