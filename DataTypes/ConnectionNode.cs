using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class ConnectionNode {
        public string LocationID;
        public string Direction;
        public int zeriCost;
        public int timeCost;
        public SkillCheckCondition SkillCondition = new();
        public ItemCondition ItemCondition = new();
        public List<DataCondition> ActiveConditions = new();
        public bool AllConditions = false;
        public bool VisibleWhileInactive = false;
        public string ReqSummary = "";
        public string CostSummary = "";

        public ConnectionNode(string l, string dir, int zeri = 0, int time = 0) {
            LocationID = l;
            Direction = dir;
            zeriCost = zeri;
            timeCost = time; 
        }
         
        public bool AllConditionsPassed() {
            bool anyTrue = false;

            for (int i = 0; i < ActiveConditions.Count; i++) {
                if (ActiveConditions[i].Valid()) {
                    anyTrue = true;
                } else {
                    if (AllConditions)
                        return false;
                }
            }

            return anyTrue;
        }

        public bool HasConditions() {
            return (SkillCondition.Skill != "" || ItemCondition.ItemID != "" || ActiveConditions.Count > 0);
        }
    }
}
