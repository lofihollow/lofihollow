using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class Feedback {
        public int Mood = 3; // 1 - 5 scale, 1 is bad, 5 is good
        public string Message = "";
    }
}
