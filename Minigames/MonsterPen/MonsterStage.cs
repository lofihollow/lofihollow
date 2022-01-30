using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames {
    [JsonObject(MemberSerialization.OptOut)]
    public class MonsterStage { 
        public string Species = "";
        public int Glyph = 0;
        public int ForeR = 0;
        public int ForeG = 0;
        public int ForeB = 0;

        public int HungerSpeed = 20; // Amount of hunger gained per day
        public int HappinessDecay = 5; // Amount of happiness lost per day

        public int StageLengthInDays = 0;

        public string RecurringDrop = "";
        public bool DropHasQuality = false;
        public int MaxDropQuality = 0;
        public int DropMinQty = 1;
        public int DropMaxQty = 1;

        public string Eats = "Anything";
    }
}
