using LofiHollow.DataTypes;
using Newtonsoft.Json; 
using System.Collections.Generic;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class Container {
        [JsonProperty]
        public int Capacity = 0;
        [JsonProperty]
        public List<Item> Items = new();
        [JsonProperty]
        public string Name = "";

        [JsonConstructor]
        public Container() { }

        public Container(int cap) {
            Capacity = cap;
        }


        public bool Add(Item item, int quantity) {
            for (int i = 0; i < Items.Count; i++) {
                if (Items[i].StacksWith(item)) {
                    if (quantity <= item.ItemQuantity) {
                        Items[i].ItemQuantity += quantity;
                    } else {
                        Items[i].ItemQuantity += item.ItemQuantity;
                    }
                    return true;
                }
            }

            if (Items.Count < Capacity) {
                Item newItem = Item.Copy(item);
                newItem.ItemQuantity = quantity;
                Items.Add(newItem);
                return true;
            }

            return false;
        }

        public Item Remove(int slot, int quantity) {
            if (slot < Items.Count) {
                if (Items[slot].IsStackable) {
                    if (quantity < Items[slot].ItemQuantity) {
                        Items[slot].ItemQuantity -= quantity;
                        Item pop = Item.Copy(Items[slot]);
                        pop.ItemQuantity = quantity;
                        return pop;
                    } else {
                        Item pop = Item.Copy(Items[slot]);
                        pop.ItemQuantity = quantity;
                        Items.RemoveAt(slot);
                        return pop;
                    }
                } else {
                    if (Items[slot].ItemQuantity > 1) {
                        Items[slot].ItemQuantity--;
                        Item pop = Item.Copy(Items[slot]);
                        pop.ItemQuantity = 1;
                        return pop;
                    } else {
                        Item pop = Item.Copy(Items[slot]);
                        pop.ItemQuantity = quantity;
                        Items.RemoveAt(slot);
                        return pop;
                    }
                }
            }

            return null;
        }
    }
}
