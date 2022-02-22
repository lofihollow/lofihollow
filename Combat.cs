using LofiHollow.DataTypes;
using LofiHollow.Entities;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
