using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class ModScript {
        public string Name = "";
        public string Package = "";
        public string Script = "";

        public string FullName() {
            return Package + ":" + Name;
        }
    }
}
