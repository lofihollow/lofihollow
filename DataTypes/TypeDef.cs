using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class TypeDef {
        public string Name = "";

        public List<string> StrongAgainst = new();
        public List<string> WeakAgainst = new();
        public List<string> ImmuneTo = new();

        public int ModDamage(int damage, string type) {
            float dmg = damage;
            if (StrongAgainst.Contains(type)) {
                return (int) Math.Ceiling(dmg / 2f);
            }

            if (WeakAgainst.Contains(type)) {
                return (int) Math.Ceiling(dmg * 2f);
            }

            if (ImmuneTo.Contains(type)) {
                return 0;
            }

            return (int) Math.Ceiling(dmg);
        }
    }
}
