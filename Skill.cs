using SadConsole;
using SadRogue.Primitives;
using System;
using ProtoBuf;

namespace LofiHollow {
    [ProtoContract]
    public class Skill {
        [ProtoMember(1)]
        public string Name = "";
        [ProtoMember(2)]
        public int Level = 1;
        [ProtoMember(3)]
        public int Experience = 0;
        [ProtoMember(4)]
        public int TotalExp = 0;

        public int ExpToLevel() {
            double exp = 0.25 * Math.Floor(Level - 1.0 + 300.0 * (Math.Pow(2.0, (Level - 1.0) / 7.0)));
            return (int)Math.Floor(exp);
        }

        public void GrantExp(int gained) {
            Experience += gained;
            if (Experience >= ExpToLevel()) {
                TotalExp += ExpToLevel();
                Experience -= ExpToLevel();
                Level++;
                GameLoop.UIManager.AddMsg(new ColoredString("You leveled " + Name + " to " + Level + "!", Color.Cyan, Color.Black));

                if (Name == "Constitution") {
                    GameLoop.World.Player.player.MaxHP = Level;
                    GameLoop.World.Player.player.CurrentHP += 1;
                }

                NetMsg skillLevel = new("updateSkill", null);
                skillLevel.MiscString = Name;
                skillLevel.MiscInt = Level;
                GameLoop.SendMessageIfNeeded(skillLevel, false, true);
            }
        }

        public Skill() {

        }

        public Skill(Skill other) {
            Name = other.Name;
            Level = other.Level;
            Experience = other.Experience;
        }
    }
}
