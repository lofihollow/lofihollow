using LofiHollow.DataTypes;
using System.Collections.Generic;

namespace LofiHollow {
    public class Combat {
        public int CombatID = 0;
        public List<CombatParticipant> Allies = new();
        public List<CombatParticipant> Enemies = new();

        public Combat() {
            CombatID = GameLoop.rand.Next();
        }
    }
}
