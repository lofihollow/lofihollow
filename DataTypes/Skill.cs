using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using System;
using LofiHollow.DataTypes;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)] 
    public class Skill {  
        public string Name = ""; 
        public int Level = 1; 
        public int Experience = 0; 
        public int TotalExp = 0;
        public bool CanLevel = true;
        public bool PermaOff = false;
        public bool IsTech = false;
        public bool IsMagic = false;
        public string Attribute = "";

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
                 

                if (Level == 92) {
                    if (GameLoop.SteamManager.UnlockAchievement("SKILL_92")) {
                        GameLoop.UIManager.AddMsg("Achievement: Halfway to Mastery!");
                    } 
                }

                if (Level == 99) {
                    if (GameLoop.SteamManager.UnlockAchievement("SKILL_99")) {
                        GameLoop.UIManager.AddMsg("Achievement: Skill Master!");
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
