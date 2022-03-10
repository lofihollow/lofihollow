using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Jobs {
    public class MinesweeperTile {
        public int Adjacent = 0;
        public bool Mine = false;
        public bool Revealed = false;
        public bool Flagged = false;
    }
}
