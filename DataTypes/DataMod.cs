using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class DataMod {
        public string Target = "";
        public string ModType = "";
        public int Amount = 0;

        public void DoMod() {
            if (ModType == "set") {
                Helper.SetPlayerData(Target, Amount);
            } else if (ModType == "mod") {
                Helper.ModifyPlayerData(Target, Amount);
            } else if (ModType == "bool") {
                Helper.PlayerDataToggleBool(Target);
            } else if (ModType == "flip") {
                Helper.PlayerDataFlipBit(Target, Amount);
            } else if (ModType == "rem") {
                Helper.ErasePlayerData(Target);
            }
        }
    }
}
