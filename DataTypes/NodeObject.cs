using LofiHollow.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class NodeObject {
        public string DisplayName = "";
        public string ID = "";
        public string Package = "";
        public string InteractVerb = "";

        public List<DataMod> DataMods = new();
        public List<string> NeedMods = new(); // format ex: "Sleep;-10"
        public List<DataCondition> ActiveConditions = new();
        public List<DataCondition> InactiveConditions = new();
        public bool AllActiveConditions = false;
        public bool AllInactiveConditions = false;
        public List<string> ItemGiven = new();
        public string TimerID = "";
        public uint TimerHours = 0; // Time to respawn, only used if above 0. 24 for a day, 168 for a week, 672 for a month, 2688 for a year
        public int zeriCost = 0;
        public int timeCost = 0;
        public string SkillReq = ""; // format ex: "Mining;10"
        public List<string> ExpGranted = new(); // format ex: "Mining;10"
        public string StartsMinigame = ""; 

        public string CostSummary = "";
        public string ReqSummary = "";
        public string NeedSummary = "";
        public string MinigameSummary = "";
        public string ActionString = "";
        public string UseMessage = "";

        public int UseMR = 255;
        public int UseMG = 255;
        public int UseMB = 255;

        public string FullName() {
            return Package + ":" + ID;
        }

        public bool HideConditionsPassed() {
            bool anyTrue = false;

            for (int i = 0; i < InactiveConditions.Count; i++) {
                if (InactiveConditions[i].Valid()) {
                    anyTrue = true;
                }
                else {
                    if (AllInactiveConditions)
                        return false;
                }
            }

            return anyTrue;
        }

        public bool AllConditionsPassed(Player p, string loc) { 
            bool anyTrue = false;

            for (int i = 0; i < ActiveConditions.Count; i++) {
                if (ActiveConditions[i].Valid()) {
                    anyTrue = true;
                }
                else {
                    if (AllActiveConditions)
                        return false;
                }
            }

            if (!anyTrue && ActiveConditions.Count > 0)
                return false;

            if (zeriCost > p.Zeri && zeriCost != 0)
                return false;
            if (TimerID != null && p.Timers.ContainsKey(loc + ";" + TimerID))
                return false;
            if (timeCost + p.Clock.GetCurrentTime() >= (p.BlackoutHour * 60))
                return false;
            if (SkillReq != null && SkillReq != "") {
                string[] split = SkillReq.Split(";");

                if (split.Length >= 2) {
                    int level = int.Parse(split[1]);

                    if (p.Skills.ContainsKey(split[0])) {
                        if (p.Skills[split[0]].Level < level)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
