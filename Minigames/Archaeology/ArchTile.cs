using Newtonsoft.Json;
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
        public int Condition = 11;

        [JsonConstructor]
        public ArchTile() { }

        public ArchTile(ArchTile other) {
            DirtPercent = other.DirtPercent;
            StonePercent = other.StonePercent;
            ForeR = other.ForeR;
            ForeG = other.ForeG;
            ForeB = other.ForeB;
            Glyph = other.Glyph;
            Condition = other.Condition;
        }

        public void Clean() {
            DirtPercent = 0;
            StonePercent = 0;
        }

        public void Brush(int strength) {
            if (DirtPercent >= strength)
                DirtPercent -= strength;
            else {
                int excess = strength - DirtPercent;
                DirtPercent = 0;

                if (Condition > 5) {
                    Condition = Math.Clamp(Condition - excess, 5, 11);
                }
            }
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

        public CellDecorator[] GetCondition() {
            CellDecorator[] decs = new CellDecorator[1];
            if (Condition == 0)
                decs[0] = new CellDecorator(new Color(0, 0, 0), 384, Mirror.None);
            else if (Condition == 1)
                decs[0] = new CellDecorator(new Color(0, 0, 0), 385, Mirror.None);
            else if (Condition == 2)
                decs[0] = new CellDecorator(new Color(0, 0, 0), 386, Mirror.None);
            else if (Condition == 3)
                decs[0] = new CellDecorator(new Color(0, 0, 0), 387, Mirror.None);
            else if (Condition == 4)
                decs[0] = new CellDecorator(new Color(0, 0, 0), 388, Mirror.None);
            else if (Condition == 5)
                decs[0] = new CellDecorator(new Color(255, 255, 255, 50), 384, Mirror.None);
            else if (Condition == 6)
                decs[0] = new CellDecorator(new Color(255, 255, 255, 50), 385, Mirror.None);
            else if (Condition == 7)
                decs[0] = new CellDecorator(new Color(255, 255, 255, 50), 386, Mirror.None);
            else if (Condition == 8)
                decs[0] = new CellDecorator(new Color(255, 255, 255, 50), 387, Mirror.None);
            else if (Condition == 9)
                decs[0] = new CellDecorator(new Color(255, 255, 255, 50), 388, Mirror.None); 

            return decs;
        }

        public CellDecorator[] GetDirtCovering() {
            CellDecorator[] decs = new CellDecorator[1];
             
            if (DirtPercent > 0) {
                if (DirtPercent == 10)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 377, Mirror.None);
                else if (DirtPercent == 9)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 376, Mirror.Vertical);
                else if (DirtPercent == 8)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 375, Mirror.Horizontal);
                else if (DirtPercent == 7)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 374, Mirror.None);
                else if (DirtPercent == 6)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 373, Mirror.Vertical);
                else if (DirtPercent == 5)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 372, Mirror.Horizontal);
                else if (DirtPercent == 4)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 371, Mirror.None);
                else if (DirtPercent == 3)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 370, Mirror.Vertical);
                else if (DirtPercent == 2)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 369, Mirror.Horizontal);
                else if (DirtPercent == 1)
                    decs[0] = new CellDecorator(new Color(155, 118, 83), 368, Mirror.None);

                return decs;
            }

            return null;
        }
    }
}
