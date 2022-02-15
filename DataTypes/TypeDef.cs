using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class TypeDef {
        public List<string> StrongAgainst = new();
        public List<string> WeakAgainst = new();
        public List<string> ImmuneTo = new();

        public int ModDamage(int damage, string type) {
            if (StrongAgainst.Contains(type)) {
                return damage / 2;
            }

            if (WeakAgainst.Contains(type)) {
                return damage * 2;
            }

            if (ImmuneTo.Contains(type)) {
                return 0;
            }

            return damage;
        }
    }
}
