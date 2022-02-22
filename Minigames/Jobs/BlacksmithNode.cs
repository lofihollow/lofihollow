using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.Minigames.Jobs {
    public class BlacksmithNode {
        public string Symbol = "";
        public int Glyph = 326;
        public Color Col;

        public bool Selected = false;

        public BlacksmithNode(BlacksmithNode other) {
            Symbol = other.Symbol;
            Glyph = other.Glyph;
            Col = other.Col;
        }

        public BlacksmithNode() {
            string[] Possible = { "A", "B", "C", "D", "E" };

            Symbol = Possible[GameLoop.rand.Next(Possible.Length)];

            if (Symbol == "A") { // Copper
                Col = new Color(183, 65, 14);
                Glyph = 18;
            } else if (Symbol == "B") { // Iron
                Col = new Color(83, 86, 90);
                Glyph = 20;
            } else if (Symbol == "C") { // Gold
                Col = new Color(255, 215, 0);
            } else if (Symbol == "D") { // Silver
                Col = new Color(192, 192, 192);
                Glyph = 321;
            } else if (Symbol == "E") { // Digitite
                Col = new Color(0, 161, 173);
                Glyph = 19;
            }
        }

        public int ClickClear(int x, int y, Dictionary<Point, BlacksmithNode> grid, bool first = false) {
            int score = -1;

            if (!first)
                score = 1;
            Selected = true;
             
            if (grid.ContainsKey(new Point(x - 1, y)) && !grid[new Point(x - 1, y)].Selected && grid[new Point(x - 1, y)].Symbol == Symbol) {
                score += grid[new Point(x - 1, y)].ClickClear(x - 1, y, grid);
            }

            if (grid.ContainsKey(new Point(x + 1, y)) && !grid[new Point(x + 1, y)].Selected && grid[new Point(x + 1, y)].Symbol == Symbol) {
                score += grid[new Point(x + 1, y)].ClickClear(x + 1, y, grid);
            }

            if (grid.ContainsKey(new Point(x, y - 1)) && !grid[new Point(x, y - 1)].Selected && grid[new Point(x, y - 1)].Symbol == Symbol) {
                score += grid[new Point(x, y - 1)].ClickClear(x, y - 1, grid);
            }

            if (grid.ContainsKey(new Point(x, y + 1)) && !grid[new Point(x, y + 1)].Selected && grid[new Point(x, y + 1)].Symbol == Symbol) {
                score += grid[new Point(x, y + 1)].ClickClear(x, y + 1, grid);
            }

            Symbol = " ";

            return score;
        }
    }
}
