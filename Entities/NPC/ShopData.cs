using SadRogue.Primitives;
using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Entities.NPC {
    public class ShopData {
        //Shop structure: Shop ID, Sold goods, Bought goods, restock time, shop MapPos and Position
        public string ShopName = "";
        public bool WanderingVendor = false;
        public Point Position;
        public Point3D MapPos;
        public List<string> SoldItems = new();
        public int BuyCategory = 0;

        public bool ShopOpen(NPC vendor) {
            if (WanderingVendor)
                return true;
            if (vendor.Position == Position && vendor.MapPos == MapPos)
                return true;
            return false;
        }

        public int GetPrice(int relationship, Item item, bool buying) {
            bool interested = BuyCategory == item.ItemCategory || SoldItems.Contains(item.Name);
            float mod = 0;

            if (relationship > 0)
                mod = Math.Max(0, (relationship / 2));
            else if (relationship < 0)
                mod = Math.Min(0, (relationship / 2));

            int price = item.AverageValue;

            
            float priceMultBase = 1.5f;

            if (buying)
                if (interested)
                    priceMultBase = 0.5f;
                else
                    priceMultBase = 0.1f;

            if (!buying)
                if (mod < 0)
                    priceMultBase = Math.Min(2.0f, 1.5f + -(mod / 100));
                else if (mod > 0)
                    priceMultBase = Math.Max(1.0f, 1.5f - (mod / 100));
            else
                if (interested)
                    if (mod < 0)
                        priceMultBase = Math.Max(0.1f, 0.5f + -(mod / 100));
                    else if (mod > 0)
                        priceMultBase = Math.Min(0.9f, 0.5f - (mod / 100));
                else
                    if (mod < 0)
                        priceMultBase = Math.Max(0.0f, 0.1f + -(mod / 100));
                    else if (mod > 0)
                        priceMultBase = Math.Min(0.5f, 0.1f - (mod / 100));

            return (int) (price * priceMultBase); 
        }
    }
}
