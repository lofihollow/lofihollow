using LofiHollow.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow {
    [JsonObject(MemberSerialization.OptIn)]
    public class ConstructionMaterial {
        [JsonProperty]
        public string Name = "";
        [JsonProperty]
        public int SubID = 0;
        [JsonProperty]
        public int ItemQuantity = 0;
        [JsonProperty]
        public bool Stacks = false;


        [JsonConstructor]
        public ConstructionMaterial() { }

        public ConstructionMaterial(string name, int qty, bool stacks) {
            Name = name;
            ItemQuantity = qty;
            Stacks = stacks;
        }

        public int ActorHasComponent(Actor act, int CraftAmount, int MinQuality) {
            int heldQty = 0;
            int Quality = 0;

            for (int i = 0; i < act.Inventory.Length; i++) {
                if (act.Inventory[i].FullName() == Name && act.Inventory[i].SubID == SubID) {
                    if (act.Inventory[i].Quality == 0 || act.Inventory[i].Quality >= MinQuality) {
                        heldQty += act.Inventory[i].ItemQuantity;

                        if (act.Inventory[i].Quality > Quality)
                            Quality = act.Inventory[i].Quality;
                    }
                }
            }

            if (heldQty >= ItemQuantity * CraftAmount)
                return Quality;

            return -1;
        }
    }
}
