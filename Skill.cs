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
    public class Skill {
        [JsonProperty]
        public int SkillID = -1;
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public int Level = 1;
        [JsonProperty]
        public int Experience = 0;
        [JsonProperty]
        public int TotalExp = 0;

        [JsonProperty]
        public List<string> Uses = new();

        public int ExpToLevel() {
            double exp = 0.25 * Math.Floor(Level - 1.0 + 300.0 * (Math.Pow(2.0, (Level - 1.0) / 7.0)));
            return (int) Math.Floor(exp);
        }

        public void GrantExp(int gained) {
            Experience += gained;
            if (Experience >= ExpToLevel()) {
                TotalExp += ExpToLevel();
                Experience -= ExpToLevel();
                Level++;
                GameLoop.UIManager.AddMsg(new ColoredString("You leveled " + Name + " to " + Level + "!", Color.Cyan, Color.Black));

                if (Name == "Constitution") {
                    GameLoop.World.Player.MaxHP = Level;
                    GameLoop.World.Player.CurrentHP += 1;
                }

                GameLoop.SendMessageIfNeeded(new string[] { "updateSkill", Name, Level.ToString() }, false, true);
            }
        }

        [JsonConstructor]
        public Skill() {

        }


        public Skill(Skill other) {
            SkillID = other.SkillID;
            Name = other.Name;
            Level = other.Level;
            Experience = other.Experience;
        }
    }
}
