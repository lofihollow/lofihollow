using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class DescriptionBlock {
        public string Description = "";
        public bool ReplacesMainBlock = false; // if false, add this to the end of the description.
        public List<DataCondition> ActiveConditions = new();
        public bool AllConditions = false;

        public bool AllConditionsPassed() {
            for (int i = 0; i < ActiveConditions.Count; i++) {
                if (AllConditions) {
                    if (!ActiveConditions[i].Valid())
                        return false;
                } else {
                    if (ActiveConditions[i].Valid())
                        return true;
                }
            }

            if (AllConditions)
                return true;
            else
                return false;
        }
    }
}
