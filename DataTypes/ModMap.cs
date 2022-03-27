using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class ModMap {
        public Map Map;
        public Point3D MapPos;

        public ModMap() {
            Map = new(); 
            MapPos = new();
        }
    }
}
