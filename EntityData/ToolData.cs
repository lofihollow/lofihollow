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

        public bool ActorHasTool(Actor act) {
            if (GameLoop.UIManager.Crafting.StationTool != "None" && GameLoop.UIManager.Crafting.StationTool == Property && GameLoop.UIManager.Crafting.StationTier >= Tier)
                return true;

            for (int i = 0; i < act.Inventory.Length; i++) {
                if (act.Inventory[i].Tool != null) {
                    for (int j = 0; j < act.Inventory[i].Tool.Count; j++) {
                        if (act.Inventory[i].Tool[j].Property == Property && act.Inventory[i].Tool[j].Tier >= Tier) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
