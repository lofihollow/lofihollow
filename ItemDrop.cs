 
namespace LofiHollow {
    public class ItemDrop {
        public string Name; // The item to be dropped
        public int DropChance; // 1 in X chance to drop this
        public int DropQuantity; // 1 - X dropped items if this item is dropped

        public ItemDrop(string name, int Chance, int Quantity) {
            Name = name;
            DropChance = Chance;
            DropQuantity = Quantity;
        }
    }
}
