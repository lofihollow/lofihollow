using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class OwnableLocation {
        public string Name;
        public bool OnlyFurniture = false;

        public OwnableLocation(string name, bool furniture) {
            Name = name;
            OnlyFurniture = furniture;
        }
    }
}
