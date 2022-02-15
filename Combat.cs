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
        public List<CSteamID> AllyIDs = new();
        public List<CSteamID> EnemyPlayers = new();

        public List<Monster> AllyMons = new();
        public List<Monster> EnemyMons = new();

        public Combat() {
            CombatID = GameLoop.rand.Next();
        }
    }
}
