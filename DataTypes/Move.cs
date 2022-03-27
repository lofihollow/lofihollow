using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class Move {
        public string Name = "Bite";
        public string Package = "lh";
        public int Power = 10;
        public int Accuracy = 80;
        public string Type = "Earth";
        public bool Physical = true;

        public string OwnStat = "";
        public int OwnStatChange = 0;

        public string EnemyStat = "";
        public int EnemyStatChange = 0;

        public string FullName() {
            return Package + ":" + Name;
        }
    }
}
