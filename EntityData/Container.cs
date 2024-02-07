using LofiHollow.DataTypes;
using Newtonsoft.Json; 
using System.Collections.Generic;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptOut)]
    public class Container {
        public string Name = "";
        public int Capacity = 0; 
        public List<Item> Items = new();

        [JsonConstructor]
        public Container() { }

        public Container(int cap) {
            Capacity = cap;
        }


        public bool Add(Item item, int quantity) {
            for (int i = 0; i < Items.Count; i++) {
                if (Items[i].StacksWith(item)) {
                    if (quantity <= item.Quantity) {
                        Items[i].Quantity += quantity;
                    } else {
                        Items[i].Quantity += item.Quantity;
                    }
                    return true;
                }
            }

            if (Items.Count < Capacity) {
                Item newItem = Item.Copy(item);
                newItem.Quantity = quantity;
                Items.Add(newItem);
                return true;
            }

            return false;
        }

        public Item Remove(int slot, int quantity) {
            if (slot < Items.Count) {
                if (Items[slot].IsStackable) {
                    if (quantity < Items[slot].Quantity) {
                        Items[slot].Quantity -= quantity;
                        Item pop = Item.Copy(Items[slot]);
                        pop.Quantity = quantity;
                        return pop;
                    } else {
                        Item pop = Item.Copy(Items[slot]);
                        pop.Quantity = quantity;
                        Items.RemoveAt(slot);
                        return pop;
                    }
                } else {
                    if (Items[slot].Quantity > 1) {
                        Items[slot].Quantity--;
                        Item pop = Item.Copy(Items[slot]);
                        pop.Quantity = 1;
                        return pop;
                    } else {
                        Item pop = Item.Copy(Items[slot]);
                        pop.Quantity = quantity;
                        Items.RemoveAt(slot);
                        return pop;
                    }
                }
            }

            return null;
        }
    }
}
