using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class DataCondition {
        public string Target = "";
        public string Check = "";
        public int CompareNum = 0;
        public int Secondary = 0;


        public bool Valid() {
            if (Check == "has") {
                return Helper.PlayerHasData(Target);
            } else if (Check == "!has") {
                return !Helper.PlayerHasData(Target);
            } else if (Check == "equals") {
                return Helper.PlayerDataEquals(Target, CompareNum);
            }
            else if (Check == "not") {
                return Helper.PlayerDataNot(Target, CompareNum);
            }
            else if (Check == "above") {
                return Helper.PlayerDataAbove(Target, CompareNum);
            }
            else if (Check == "below") {
                return Helper.PlayerDataBelow(Target, CompareNum);
            }
            else if (Check == "bool") {
                return Helper.PlayerDataAsBool(Target);
            }
            else if (Check == "e<>") {
                return Helper.PlayerDataAbove(Target, CompareNum) && Helper.PlayerDataBelow(Target, Secondary);
            }
            else if (Check == "e<!>") {
                return Helper.PlayerDataBelow(Target, CompareNum) || Helper.PlayerDataAbove(Target, Secondary);
            }
            else if (Check == "i<>") {
                return Helper.PlayerDataAbove(Target, CompareNum - 1) && Helper.PlayerDataBelow(Target, Secondary + 1);
            }
            else if (Check == "i<!>") {
                return Helper.PlayerDataBelow(Target, CompareNum + 1) && Helper.PlayerDataAbove(Target, Secondary - 1);
            }

            return false;
        }
    }
}
