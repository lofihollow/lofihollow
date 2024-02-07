using System;
using System.Collections.Generic;
using System.Linq;
using LofiHollow.DataTypes;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using LofiHollow.Minigames;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using Steamworks;

namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptOut)]
    public class Monster : StatBall {  
        public string Package = "";  
           
        public int Level = 0; 
        public string StatGranted = "";
         
        public List<string> MoveList = new(); 
        public List<string> DropTable = new(); 

        [JsonConstructor]
        public Monster(Color fore) : base(fore) { 
        }

        public static Monster Clone(string name) {
            if (GameLoop.World.monsterLibrary != null && GameLoop.World.monsterLibrary.ContainsKey(name)) {
                Monster temp = Helper.Clone(GameLoop.World.monsterLibrary[name]);  

                return temp;
            }

            return null;
        }

        public static Monster Clone(Monster other) { 
            Monster temp = Helper.Clone(other); 

            return temp;  
        } 

        public static int GetStat(int input, int Level, int Growth, int StatEXP, bool HP = false) {
            int trainingBonus = (int) Math.Floor((float) StatEXP / 4f);
            if (HP) {
                int hp = (int)Math.Floor(0.01 * ((2 * input) + Growth + trainingBonus + Level)) + Level + 10;
                return hp;
            }
            else {
                int stat = (int)Math.Floor(0.01 * ((2 * input) + Growth + trainingBonus + Level) + 5);
                return stat;
            }
        }  
    }
}
