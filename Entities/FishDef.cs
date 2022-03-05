using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities {
    public class FishColor {
        public int R = 0;
        public int G = 0;
        public int B = 0;
        public int A = 255;
        public string Name = "";

        public FishColor(string n, int r, int g, int b, int a = 255) {
            Name = n;
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public FishColor(string n, Color col) {
            Name = n;
            R = col.R;
            G = col.G;
            B = col.B;
            A = col.A;
        }
    }

    public class FishTime {
        public string Name = "";
        public int StartTime = 360;
        public int EndTime = 360;

        public FishTime(string name, int start, int end) {
            Name = name;
            StartTime = start;
            EndTime = end;
        }
    }

    public class FishSpecies {
        public string Name = "";
        public int Glyph = 0;

        public FishSpecies(string name, int gly) {
            Name = name;
            Glyph = gly;
        }
    }

    public class FishBehavior {
        public string Name = "";
        public int FightChance = 0;
        public int FightLength = 0;
        public int Strength = 0;

        public FishBehavior(string name, int chance, int length, int str) {
            Name = name;
            FightChance = chance;
            FightLength = length;
            Strength = str;
        }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class FishDef {
        [JsonProperty]
        public string PackageID = "";
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public string Season = "";
        [JsonProperty]
        public int EarliestTime = 360;
        [JsonProperty]
        public int LatestTime = 360;
        [JsonProperty]
        public string CatchLocation = "";

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
        public string FishItemID = "";
        [JsonProperty]
        public string FilletID = "";

        public ColoredString GetAppearance() {
            return new ColoredString(glyph.AsString(), new Color(colR, colG, colB), Color.Black);
        }

        public string FullName() {
            return PackageID + ":" + Name;
        }
    }
}
