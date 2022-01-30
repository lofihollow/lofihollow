using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Jobs {
    public class BarPatron {
        public int Happiness = 11;
        public int ForeR = 0;
        public int ForeG = 0;
        public int ForeB = 0;
        public string DesiredDrink = "";
        public int X = 0;

        public BarPatron() {
            ForeR = GameLoop.rand.Next(255);
            ForeG = GameLoop.rand.Next(255);
            ForeB = GameLoop.rand.Next(255);
        }

        public Color GetColor() {
            return new Color(ForeR, ForeG, ForeB);
        }

        public string GetDrink() {
            if (DesiredDrink == "Beer")
                return ((char)340).ToString();
            if (DesiredDrink == "Wine")
                return ((char)341).ToString();
            if (DesiredDrink == "Whiskey")
                return ((char)342).ToString();
            if (DesiredDrink == "Martini")
                return ((char)343).ToString();
            return ((char)340).ToString();
        }

        public ColoredString Mood() {
            if (Happiness > 7)
                return new ColoredString(((char)344).ToString(), Color.Cyan, Color.Black);
            if (Happiness > 4)
                return new ColoredString(((char)345).ToString(), Color.Green, Color.Black);
            if (Happiness > 1)
                return new ColoredString(((char)346).ToString(), Color.Yellow, Color.Black);
            return new ColoredString(((char)347).ToString(), Color.Red, Color.Black);
        }

        public bool CheckDrink(string front) {
            if (DesiredDrink == front)
                return true;
            return false;
        }

        public int Tip() {
            if (Happiness > 7)
                return 4;
            if (Happiness > 4)
                return 3;
            if (Happiness > 1)
                return 2;
            if (Happiness > 0)
                return 1;
            return 0;
        }
    }
}
