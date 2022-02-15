using SadConsole; 

namespace LofiHollow.DataTypes {
    public class DecoratedString {
        public ColoredString Text;
        public CellDecorator[] Decorators;


        public DecoratedString(ColoredString text) {
            Text = text;
        }

        public DecoratedString(ColoredString text, CellDecorator[] decs) {
            Text = text;
            Decorators = decs;
        }
    }
}
