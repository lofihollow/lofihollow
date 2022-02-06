using LofiHollow.Entities;
using Newtonsoft.Json;
using ProtoBuf;

namespace LofiHollow.EntityData {
    [ProtoContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ConstructionMaterial {
        [ProtoMember(1)]
        public string Name = "";
        [ProtoMember(2)]
        public int ItemQuantity = 0;
        [ProtoMember(3)]
        public bool Stacks = false;


        [JsonConstructor]
        public ConstructionMaterial() { }

        public ConstructionMaterial(string name, int qty, bool stacks) {
            Name = name;
            ItemQuantity = qty;
            Stacks = stacks;
        }

        public int ActorHasComponent(Player act, int CraftAmount, int MinQuality) {
            int heldQty = 0;
            int Quality = 0;

            for (int i = 0; i < act.Inventory.Length; i++) {
                if (act.Inventory[i].FullName() == Name) {
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
