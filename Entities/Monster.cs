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
    [JsonObject(MemberSerialization.OptOut)]
    public class Monster { 
        public string Species = ""; 
        public string Package = "";
        public string UniqueID;
         
        public int Health = 1; 
        public int Speed = 1; 
        public int Attack = 1; 
        public int Defense = 1;  
        public int MAttack = 1; 
        public int MDefense = 1;
         
        public int MinLevel = 1; 
        public int MaxLevel = 99;
         
        public int CaptureRate = 45;
         
        public string StatGrowths = "";

        public int EvolutionLevel = 10;
        public string EvolvesInto = "";
        public string AlternateEvoMethod = "";
         
        public string SpawnLocation = "";

        public string ElementalAlignment = "Fire";
        public List<string> Types = new();
        public List<string> MoveList = new(); 
        public List<string> DropTable = new();
          
        public int ForegroundR = 0; 
        public int ForegroundG = 0; 
        public int ForegroundB = 0; 
        public int ForegroundA = 255; 
        public int ActorGlyph = 0;  

        [JsonConstructor]
        public Monster() { 
        }

        public static Monster Clone(string name) {
            if (GameLoop.World.monsterLibrary != null && GameLoop.World.monsterLibrary.ContainsKey(name)) {
                Monster temp = Helper.Clone(GameLoop.World.monsterLibrary[name]); 
                temp.UniqueID = Guid.NewGuid().ToString("N");

                return temp;
            }

            return null;
        }

        public static Monster Clone(Monster other) { 
            Monster temp = Helper.Clone(other);
            temp.UniqueID = Guid.NewGuid().ToString("N");

            return temp;  
        }
         

        public ColoredString GetAppearance() {
            return new ColoredString(ActorGlyph.AsString(), new Color(ForegroundR, ForegroundG, ForegroundB, ForegroundA), Color.Black);
        }

        public CellDecorator AsDecorator() {
            return new CellDecorator(new Color(ForegroundR, ForegroundG, ForegroundB, ForegroundA), ActorGlyph, Mirror.Horizontal);
        } 

        public string FullName() {
            return Package + ":" + Species;
        }
    }
}
