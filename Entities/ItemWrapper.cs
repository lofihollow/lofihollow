using SadConsole;
using SadRogue.Primitives;
using LofiHollow.DataTypes;

namespace LofiHollow.Entities {
    public class ItemWrapper : Entity {
        public Item item;
        public int CurrentAlpha = 255;

        public ItemWrapper(string name) : base(Color.Black, Color.Transparent, 32) {
            if (GameLoop.World.itemLibrary.ContainsKey(name)) {
                item = Item.Copy(name);

                Appearance.Foreground = new(item.ForegroundR, item.ForegroundG, item.ForegroundB, CurrentAlpha);
                Appearance.Glyph = item.ItemGlyph;
            }
        }

        public ItemWrapper(Item temp) : base(Color.Black, Color.Transparent, 32) { 
                item = Item.Copy(temp);

            Appearance.Foreground = new(item.ForegroundR, item.ForegroundG, item.ForegroundB, CurrentAlpha);
            Appearance.Glyph = item.ItemGlyph; 
        }

        public void UpdateAppearance() {
            Appearance.Foreground = new(item.ForegroundR, item.ForegroundG, item.ForegroundB, CurrentAlpha);
            Appearance.Glyph = item.ItemGlyph;
        }

        public ColoredString AsColoredGlyph() {
            ColoredString output = new(item.ItemGlyph.AsString(), new Color(item.ForegroundR, item.ForegroundG, item.ForegroundB, CurrentAlpha), Color.Transparent);
            return output;
        }

        public CellDecorator GetDecorator() {
            CellDecorator dec = new(new Color(item.Dec.R, item.Dec.G, item.Dec.B, item.Dec.A), item.Dec.Glyph, Mirror.None);
            return dec;
        }
    }
}
