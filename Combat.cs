using LofiHollow.DataTypes;
using SadConsole;
using Steamworks;
using System.Collections.Generic;

namespace LofiHollow {
    public class Combat {
        public int CombatID = 0;
         
        // public SteamId BattleHost;
        // public List<SteamId> OtherPlayers = new();
         
        public List<Item> Drops = new();

        public Combat() {
            CombatID = GameLoop.rand.Next();
        }
    }
}
