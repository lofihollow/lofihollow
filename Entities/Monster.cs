using System;
using System.Collections.Generic;
using System.Linq;
using LofiHollow.Managers;
using LofiHollow.Minigames;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptIn)]
    public class Monster {
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public string Package = "";

        [JsonProperty]
        public string UniqueID;

        [JsonProperty]
        public int MonConstitution = 1;
        [JsonProperty]
        public int MonAttack = 1;
        [JsonProperty]
        public int MonStrength = 1;
        [JsonProperty]
        public int MonDefense = 1;
        [JsonProperty]
        public int MonMagic = 1;
        [JsonProperty]
        public int MonRanged = 1;

        [JsonProperty]
        public string CombatType = "";
        [JsonProperty]
        public string DamageType = "Crush";
        [JsonProperty]
        public string SpecificWeakness = "";
        [JsonProperty]
        public string SpawnLocation = "";

        [JsonProperty]
        public int Confidence = 0;

        [JsonProperty]
        public bool AlwaysAggro = false;

        [JsonProperty]
        public bool CanDropEgg = false;
        [JsonProperty]
        public Egg EggData;

        [JsonProperty]
        public List<string> DropTable = new();

        [JsonProperty]
        public int ForegroundR = 0;
        [JsonProperty]
        public int ForegroundG = 0;
        [JsonProperty]
        public int ForegroundB = 0;
        [JsonProperty]
        public int ForegroundA = 0;
        [JsonProperty]
        public int ActorGlyph = 0;


        [JsonConstructor]
        public Monster() { 
        }

        public Monster(string name) {
            if (GameLoop.World.monsterLibrary != null && GameLoop.World.monsterLibrary.ContainsKey(name)) {
                Monster temp = GameLoop.World.monsterLibrary[name];
                SetAll(temp);
            }
        }

        public ColoredString GetAppearance() {
            return new ColoredString(((char)ActorGlyph).ToString(), new Color(ForegroundR, ForegroundG, ForegroundB, ForegroundA), Color.Black);
        }

        public void SetAll(Monster temp) { 
            UniqueID = Guid.NewGuid().ToString("N");

            Name = temp.Name;
            Package = temp.Package;

            MonConstitution = temp.MonConstitution;
            MonAttack = temp.MonAttack;
            MonStrength = temp.MonStrength;
            MonDefense = temp.MonDefense;
            MonMagic = temp.MonMagic;
            MonRanged = temp.MonRanged;

            CombatType = temp.CombatType;
            SpecificWeakness = temp.SpecificWeakness;
            DamageType = temp.DamageType;
            SpawnLocation = temp.SpawnLocation;

            CanDropEgg = temp.CanDropEgg;
            EggData = temp.EggData;

            ForegroundR = temp.ForegroundR;
            ForegroundG = temp.ForegroundG;
            ForegroundB = temp.ForegroundB;
            ActorGlyph = temp.ActorGlyph;
        }

        public string FullName() {
            return Package + ":" + Name;
        }
    }
}
