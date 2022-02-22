using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Archaeology {
    public class ArchTile {
        public int DirtPercent = 0;
        public int StonePercent = 0;

        public int ForeR = 0;
        public int ForeG = 0;
        public int ForeB = 0;
        public int Glyph = 32;

        // 0: Worthless,
        // 1: Shattered, 2: Many Cracks, 3: Some Cracks, 4: Cracked
        // 5: Very Scratched, 6: Many Scratches, 7: Some Scratches, 8: Scratched
        // 9: Nearly perfect, 11: Pristine
        public int Condition = 0;

        public void Clean() {
            DirtPercent = 0;
            StonePercent = 0;
        }

        public bool Vital() {
            bool Vital = false;

            if (Glyph != 32 && Glyph != 0) {
                Vital = true;
            }

            return Vital;
        }

        public ColoredString GetAppearance() {
            return new ColoredString(Glyph.AsString(), new Color(ForeR, ForeG, ForeB), Color.Black);
        }

        public CellDecorator[] GetDirtCovering() {
            CellDecorator[] decs = new CellDecorator[1];
            
            if (DirtPercent > 0) {
                if (DirtPercent == 100)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 377, Mirror.None);
                else if (DirtPercent > 90)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 376, Mirror.None);
                else if (DirtPercent > 80)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 375, Mirror.None);
                else if (DirtPercent > 70)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 374, Mirror.None);
                else if (DirtPercent > 60)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 373, Mirror.None);
                else if (DirtPercent > 50)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 372, Mirror.None);
                else if (DirtPercent > 40)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 371, Mirror.None);
                else if (DirtPercent > 30)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 370, Mirror.None);
                else if (DirtPercent > 20)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 369, Mirror.None);
                else if (DirtPercent > 10)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 368, Mirror.None);
                else if (DirtPercent > 0)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 367, Mirror.None);
            }

            return null;
        }
    }
}
