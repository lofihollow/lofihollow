using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class NodeItem {
        public string itemID;
        public int zeriCost;
        public int timeCost;
        public int resetDays;
        public bool shopItem;

        public NodeItem(string id, int zeri, int time, int days, bool shop) {
            itemID = id;
            zeriCost = zeri;
            timeCost = time;
            resetDays = days;
            shopItem = shop;
        }
    }
}
