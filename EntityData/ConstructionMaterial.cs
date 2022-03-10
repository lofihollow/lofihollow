using LofiHollow.Entities;
using Newtonsoft.Json; 

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class ConstructionMaterial {
        [JsonProperty]
        public string ID = "";
        [JsonProperty]
        public int ItemQuantity = 0; 


        [JsonConstructor]
        public ConstructionMaterial() { }

        public ConstructionMaterial(string name, int qty, bool stacks) {
            ID = name;
            ItemQuantity = qty; 
        }

        public int ActorHasComponent(Player act, int CraftAmount, int MinQuality) {
            int heldQty = 0;
            int Quality = 0;

            for (int i = 0; i < act.Inventory.Length; i++) {
                if (act.Inventory[i].FullName() == ID) {
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
