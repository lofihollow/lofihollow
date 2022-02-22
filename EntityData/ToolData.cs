using LofiHollow.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class ToolData {
        [JsonProperty]
        public string Property = "";
        [JsonProperty]
        public int Tier = 1;

        [JsonConstructor]
        public ToolData() { }

        public ToolData(string prop, int tier) {
            Property = prop;
            Tier = tier;
        }

        public bool ActorHasTool(Player act) {
            if (GameLoop.UIManager.Crafting.StationTool != "None" && GameLoop.UIManager.Crafting.StationTool == Property && GameLoop.UIManager.Crafting.StationTier >= Tier)
                return true;

            for (int i = 0; i < act.Inventory.Length; i++) {
                if (act.Inventory[i].Tool != null) {
                    List<ToolData> tool = act.Inventory[i].Tool;
                    for (int j = 0; j < tool.Count; j++) {
                        if (tool[j].Property == Property && tool[j].Tier >= Tier) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
