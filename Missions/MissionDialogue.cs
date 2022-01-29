using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Missions {
    [JsonObject(MemberSerialization.OptOut)]
    public class MissionDialogue {
        public string Text = "";
        public List<Response> Responses = new();
    }
}
