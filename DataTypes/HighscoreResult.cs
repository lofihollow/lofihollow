using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class HighscoreResult {
        public byte Success = 0;
        public int GlobalRank = -1;
        public int PreviousRank = -1;
        public int HighScore = -1;
        public byte NewHigh = 0;

        public HighscoreResult(byte suc, int rank, int prevRank, int score, byte newHigh) {
            Success = suc;
            GlobalRank = rank;
            PreviousRank = prevRank;
            HighScore = score;
            NewHigh = newHigh;
        }
    }
}
