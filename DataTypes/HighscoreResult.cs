using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class HighscoreResult {
        public bool Success = false;
        public int GlobalRank = -1;
        public int PreviousRank = -1;
        public int HighScore = -1;
        public bool NewHigh = false;

        public HighscoreResult(bool success, int rank, int prevRank, int score, bool newHigh) {
            Success = success;
            GlobalRank = rank;
            PreviousRank = prevRank;
            HighScore = score;
            NewHigh = newHigh;
        }
    }
}
