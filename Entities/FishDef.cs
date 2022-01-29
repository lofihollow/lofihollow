using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptIn)]
    public class FishDef {
        [JsonProperty]
        public string PackageID = "";
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public string Season = "";
        [JsonProperty]
        public int EarliestTime = 0;
        [JsonProperty]
        public int LatestTime = 0;
        [JsonProperty]
        public string CatchLocation = "";
        [JsonProperty]
        public double Weight = 0;
        [JsonProperty]
        public int AverageValue = 2;

        [JsonProperty]
        public int MaxQuality = 1; 

        [JsonProperty]
        public int RequiredLevel = 0;
        [JsonProperty]
        public int GrantedExp = 0;

        [JsonProperty]
        public int Strength = 0;
        [JsonProperty]
        public int FightChance = 10;
        [JsonProperty]
        public int FightLength = 0;

        [JsonProperty]
        public int colR = 0;
        [JsonProperty]
        public int colG = 0;
        [JsonProperty]
        public int colB = 0;
        [JsonProperty]
        public int colA = 255;
        [JsonProperty]
        public int glyph = 0;

        [JsonProperty]
        public Item RawFish;
        [JsonProperty]
        public string FilletName;

        public ColoredString GetAppearance() {
            return new ColoredString(((char) glyph).ToString(), new Color(colR, colG, colB), Color.Black);
        }
    }
}
