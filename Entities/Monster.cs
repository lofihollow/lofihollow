using System;
using System.Collections.Generic;
using System.Linq;
using LofiHollow.DataTypes;
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
        public int Health = 1;
        [JsonProperty]
        public int Speed = 1;
        [JsonProperty]
        public int Attack = 1;
        [JsonProperty]
        public int Defense = 1; 
        [JsonProperty]
        public int MAttack = 1;
        [JsonProperty]
        public int MDefense = 1;

        [JsonProperty]
        public int MinLevel = 1;
        [JsonProperty]
        public int MaxLevel = 99;

        [JsonProperty]
        public int CaptureRate = 45;

        [JsonProperty]
        public string StatGrowths = ""; 

        [JsonProperty]
        public List<string> Types = new();
        [JsonProperty]
        public string SpawnLocation = "";

        [JsonProperty]
        public List<string> MoveList = new();

        [JsonProperty]
        public List<string> DropTable = new();

        [JsonProperty]
        public int ForegroundR = 0;
        [JsonProperty]
        public int ForegroundG = 0;
        [JsonProperty]
        public int ForegroundB = 0;
        [JsonProperty]
        public int ForegroundA = 255;
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
            return new ColoredString(ActorGlyph.AsString(), new Color(ForegroundR, ForegroundG, ForegroundB, ForegroundA), Color.Black);
        }

        public CellDecorator AsDecorator() {
            return new CellDecorator(new Color(ForegroundR, ForegroundG, ForegroundB, ForegroundA), ActorGlyph, Mirror.Horizontal);
        }

        public void SetAll(Monster temp) { 
            UniqueID = Guid.NewGuid().ToString("N");

            Name = temp.Name;
            Package = temp.Package;

            Speed = temp.Speed;
            Attack = temp.Attack;
            Defense = temp.Defense;
            Health = temp.Health;
            MAttack = temp.MAttack;
            MDefense = temp.MDefense;
            StatGrowths = temp.StatGrowths;

            MinLevel = temp.MinLevel;
            MaxLevel = temp.MaxLevel;
            CaptureRate = temp.CaptureRate;

            Types = temp.Types;
            SpawnLocation = temp.SpawnLocation;
            MoveList = temp.MoveList;
            DropTable = temp.DropTable;
             
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
