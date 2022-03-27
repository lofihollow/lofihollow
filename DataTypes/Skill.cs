using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using LofiHollow.DataTypes;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptIn)] 
    public class Skill { 
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public int Level = 1;
        [JsonProperty]
        public int Experience = 0;
        [JsonProperty]
        public int TotalExp = 0; 

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
                GameLoop.SoundManager.PlaySound("ding");
                if (Name == "Constitution") {
                    GameLoop.World.Player.MaxHP = Level;
                    GameLoop.World.Player.CurrentHP += 1;
                }

                NetMsg updateSkill = new("updateSkill");
                updateSkill.MiscString1 = Name;
                updateSkill.MiscInt = Level;

                GameLoop.SendMessageIfNeeded(updateSkill, false, true);

                if (Level == 92) {
                    if (GameLoop.SteamManager.UnlockAchievement(Name.ToUpper().Replace(' ', '_') + "_92")) {
                        GameLoop.UIManager.AddMsg("Achievement: Halfway to " + Name + " Mastery!");
                    } 
                }

                if (Level == 99) {
                    if (GameLoop.SteamManager.UnlockAchievement(Name.ToUpper().Replace(' ', '_') + "_99")) {
                        GameLoop.UIManager.AddMsg("Achievement: " + Name + " Master!");
                    }
                }
            }
        }

        [JsonConstructor]
        public Skill() {

        }


        public Skill(Skill other) {
            Name = other.Name;
            Level = other.Level;
            Experience = other.Experience;
        }
    }
}
