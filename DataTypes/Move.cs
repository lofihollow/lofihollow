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
        public int Power = 10;
        public int Accuracy = 80;
        public string Type = "Crushing";
    }
}
