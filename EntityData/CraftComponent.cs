using LofiHollow.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class CraftComponent {
        [JsonProperty]
        public string Property = "";
        [JsonProperty]
        public int Tier = 0;
        [JsonProperty]
        public int Quantity = 0; 
        [JsonProperty]
        public bool CountsAsMultiple = false;
         
        [JsonConstructor]
        public CraftComponent() { }
         
        public CraftComponent(string prop, int tier, int qty, bool stack, bool countsAsMult) {
            Property = prop;
            Tier = tier;
            Quantity = qty; 
            CountsAsMultiple = countsAsMult;
        }



        public int ActorHasComponent(Player act, int CraftAmount, int MinQuality) {
            int heldQty = 0;
            int heldTotal = 0;
            int Quality = 0;
            for (int i = 0; i < act.Inventory.Length; i++) {
                if (act.Inventory[i].Craft != null) {
                    List<CraftComponent> craft = act.Inventory[i].Craft;
                    for (int j = 0; j < craft.Count; j++) {
                        if (craft[j].Property == Property && craft[j].Tier >= Tier && (act.Inventory[i].Quality == 0 || act.Inventory[i].Quality >= MinQuality)) {
                            if (CountsAsMultiple)
                                heldTotal += craft[j].Tier;
                            heldQty += act.Inventory[i].ItemQuantity;

                            if (act.Inventory[i].Quality > Quality)
                                Quality = act.Inventory[i].Quality;
                        }
                    }
                }
            }

            if (CountsAsMultiple) {
                if (heldTotal >= Tier * CraftAmount)
                    return Quality;
            } else {
                if (heldQty >= Quantity * CraftAmount)
                    return Quality;
            }

            return -1;
        }
    }
}
